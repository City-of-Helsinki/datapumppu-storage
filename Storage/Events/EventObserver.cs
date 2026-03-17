using Azure.Messaging.ServiceBus;
using Storage.Actions;
using Storage.Controllers.Event.DTOs;
using Storage.Repositories.Providers;

namespace Storage.Events
{
    /// <summary>
    /// Background service that continuously processes incoming events from Azure Service Bus.
    /// Deserializes event messages, dispatches them to appropriate action handlers, and manages database transactions.
    /// Note: Azure Service Bus is deprecated in favor of Kafka. This class is maintained for backward compatibility.
    /// </summary>
    public class EventObserver : BackgroundService
    {
        private readonly ILogger<EventObserver> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly IDatabaseConnectionFactory _connectionFactory;

        /// <summary>
        /// Initializes a new instance of the EventObserver with required dependencies.
        /// </summary>
        /// <param name="logger">Logger for recording processing status and errors.</param>
        /// <param name="serviceProvider">Service provider for creating scoped dependencies per message.</param>
        /// <param name="configuration">Application configuration containing Service Bus connection details.</param>
        /// <param name="connectionFactory">Factory for creating database connections.</param>
        public EventObserver(ILogger<EventObserver> logger, IServiceProvider serviceProvider, IConfiguration configuration, IDatabaseConnectionFactory connectionFactory)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Starts the Azure Service Bus processor and begins listening for incoming events.
        /// Configures the processor with message and error handlers.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token for graceful shutdown.</param>
        /// <returns>A task representing the background processing operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };
            var client = new ServiceBusClient(_configuration["SB_CONNECTION_STRING"], clientOptions);
            var processor = client.CreateProcessor(_configuration["SB_QUEUE_NAME"], new ServiceBusProcessorOptions());
            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;
            await processor.StartProcessingAsync(stoppingToken);
        }

        /// <summary>
        /// Handles each incoming Service Bus message.
        /// Deserializes the event, retrieves appropriate action handlers, executes them within a database transaction,
        /// and completes the message to remove it from the queue.
        /// </summary>
        /// <param name="args">Event arguments containing the message and completion methods.</param>
        /// <returns>A task representing the asynchronous message processing.</returns>
        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToObjectFromJson<EventDTO>();
            using var scope = _serviceProvider.CreateScope();

            var eventActions = scope.ServiceProvider.GetService<IEventActions>();
            var actions = eventActions.GetActionsForEvent(body.EventType);

            using var connection = await _connectionFactory.CreateOpenConnection();
            using var transaction = connection.BeginTransaction();
            try
            {
                var eventId = Guid.NewGuid();
                foreach (var action in actions)
                {
                   await action.Execute(args.Message.Body, eventId, connection, transaction);
                }

                transaction.Commit();
                _logger.LogInformation("Event successfully stored.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Transaction failed: " + ex.Message);
                transaction.Rollback();
            }

            await args.CompleteMessageAsync(args.Message);
        }

        /// <summary>
        /// Handles errors that occur during message processing.
        /// Logs the error details for troubleshooting.
        /// </summary>
        /// <param name="args">Event arguments containing error information.</param>
        /// <returns>A completed task.</returns>
        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception.ToString());
            return Task.CompletedTask;
        }

    }
}
