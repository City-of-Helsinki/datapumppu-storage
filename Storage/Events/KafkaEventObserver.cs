using Confluent.Kafka;
using Storage.Actions;
using Storage.Controllers.Event.DTOs;
using Storage.Repositories.Providers;
using Storage.Events.Providers;
using System.Text.Json;

namespace Storage.Events
{
    /// <summary>
    /// Background service that continuously processes incoming events from Kafka.
    /// Consumes messages from a Kafka topic, dispatches them to appropriate action handlers,
    /// manages database transactions, and publishes notifications to a producer topic.
    /// This is the primary event processing mechanism, replacing Azure Service Bus.
    /// </summary>
    public class KafkaEventObserver : BackgroundService
    {
        private readonly ILogger<KafkaEventObserver> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private IHostEnvironment _hostEnvironment;
        private readonly IKafkaClientFactory _clientFactory;


        /// <summary>
        /// Initializes a new instance of the KafkaEventObserver with required dependencies.
        /// </summary>
        /// <param name="logger">Logger for recording processing status and errors.</param>
        /// <param name="serviceProvider">Service provider for creating scoped dependencies per message.</param>
        /// <param name="configuration">Application configuration containing Kafka topics and connection details.</param>
        /// <param name="connectionFactory">Factory for creating database connections.</param>
        /// <param name="hostEnvironment">Host environment information.</param>
        /// <param name="clientFactory">Factory for creating Kafka consumers and producers.</param>
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

        /// <summary>
        /// Starts the Kafka event processing loop in a background thread.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token for graceful shutdown.</param>
        /// <returns>A task representing the background processing operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await MessageHandler(stoppingToken);
        }

        /// <summary>
        /// Main Kafka message processing loop.
        /// Subscribes to the consumer topic, consumes messages, deserializes events, dispatches to action handlers,
        /// commits the database transaction and Kafka offset, and publishes a notification to the producer topic.
        /// Handles errors by rolling back transactions and recreating Kafka clients as needed.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token for shutting down the processing loop.</param>
        private async Task MessageHandler(CancellationToken stoppingToken)
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
                        consumer.Subscribe(consumerTopic);
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
                    var jsonBody = JsonSerializer.Serialize(new { body.MeetingID, body.CaseNumber });
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
