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
            var topic = _configuration["KAFKA_CONSUMER_TOPIC"];
            var consumer = _clientFactory.CreateConsumer();

            consumer.Subscribe(topic);

            using var connection = await _connectionFactory.CreateOpenConnection();
            using var transaction = connection.BeginTransaction();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(stoppingToken);
                    var body = JsonSerializer.Deserialize<EventDTO>(cr.Message.Value)!;
                    using var scope = _serviceProvider.CreateScope();

                    // convert object to binarydata for azure service bus compatibility
                    var binaryBody = BinaryData.FromObjectAsJson<EventDTO>(body);

                    var eventActions = scope.ServiceProvider.GetService<IEventActions>();
                    var actions = eventActions.GetActionsForEvent(body.EventType);

                    var eventId = Guid.NewGuid();
                    foreach (var action in actions)
                    {
                        await action.Execute(binaryBody, eventId, connection, transaction);
                    }

                    consumer.Commit();
                    transaction.Commit();
                    _logger.LogInformation("Consumer Event successfully stored.");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Consumer Operation Canceled.");
                    transaction.Rollback();
                    break;
                }
                catch (ConsumeException e)
                {
                    _logger.LogError("Consumer Error: " + e.Message);
                    transaction.Rollback();
                }
                catch (Exception e)
                {
                    _logger.LogError("Consumer Unexpected Error: " + e.Message);
                    transaction.Rollback();
                }
            }
        }

    }
}
