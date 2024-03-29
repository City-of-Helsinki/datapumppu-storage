using Confluent.Kafka;
using Storage.Actions;
using Storage.Controllers.Event.DTOs;
using Storage.Repositories.Providers;
using Storage.Events.Providers;
using System.Text.Json;

namespace Storage.Events
{
    public class KafkaEventObserver : BackgroundService
    {
        private readonly ILogger<KafkaEventObserver> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private IHostEnvironment _hostEnvironment;
        private readonly IKafkaClientFactory _clientFactory;


        public KafkaEventObserver(
            ILogger<KafkaEventObserver> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IDatabaseConnectionFactory connectionFactory,
            IHostEnvironment hostEnvironment,
            IKafkaClientFactory clientFactory
        )
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _connectionFactory = connectionFactory;
            _hostEnvironment = hostEnvironment;
            _clientFactory = clientFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => MessageHandler(stoppingToken), stoppingToken);
        }

        private async void MessageHandler(CancellationToken stoppingToken)
        {
            var consumerTopic = _configuration["KAFKA_CONSUMER_TOPIC"];
            var consumer = _clientFactory.CreateConsumer();

            var producerTopic = _configuration["KAFKA_PRODUCER_TOPIC"];
            var producer = _clientFactory.CreateProducer();

            consumer.Subscribe(consumerTopic);

            using var connection = await _connectionFactory.CreateOpenConnection();

            bool recreatedKafkaClients = false;
            while (!stoppingToken.IsCancellationRequested)
            {
                using var transaction = connection.BeginTransaction();
                try
                {
                    if (recreatedKafkaClients)
                    {
                        consumer = _clientFactory.CreateConsumer();
                        producer = _clientFactory.CreateProducer();
                        recreatedKafkaClients = false;
                    }

                    var cr = consumer.Consume(stoppingToken);
                    var body = JsonSerializer.Deserialize<EventDTO>(cr.Message.Value)!;
                    _logger.LogInformation("event for meeting {0}", body.MeetingID);

                    using var scope = _serviceProvider.CreateScope();

                    var binaryBody = BinaryData.FromString(cr.Message.Value);
                    var eventActions = scope.ServiceProvider.GetService<IEventActions>();
                    var actions = eventActions.GetActionsForEvent(body.EventType);

                    var eventId = Guid.NewGuid();
                    foreach (var action in actions)
                    {
                        await action.Execute(binaryBody, eventId, connection, transaction);
                    }

                    transaction.Commit();
                    consumer.Commit(cr);

                    _logger.LogInformation("Consumer Event successfully stored.");

                    // send MeetingID to WebApi
                    var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(new { body.MeetingID, body.CaseNumber });
                    await producer.ProduceAsync(producerTopic, new Message<Null, string> { Value = jsonBody });
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Consumer Operation Canceled.");
                    transaction.Rollback();
                    recreatedKafkaClients = true;
                }
                catch (ConsumeException e)
                {
                    _logger.LogError("Consumer Error: " + e.Message);
                    transaction.Rollback();
                    recreatedKafkaClients = true;
                }
                catch (Exception e)
                {
                    _logger.LogError("Kafka Unexpected Error: " + e.Message);
                    transaction.Rollback();
                }
            }
        }

    }
}
