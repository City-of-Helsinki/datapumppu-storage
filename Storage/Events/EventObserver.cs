using Azure.Messaging.ServiceBus;
using Storage.Actions;
using Storage.Controllers.Event.DTOs;
using Storage.Repositories.Providers;

namespace Storage.Events
{
    public class EventObserver : BackgroundService
    {
        private readonly ILogger<EventObserver> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public EventObserver(ILogger<EventObserver> logger, IServiceProvider serviceProvider, IConfiguration configuration, IDatabaseConnectionFactory connectionFactory)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _connectionFactory = connectionFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };
            var client = new ServiceBusClient(_configuration["ServiceBus:ConnectionString"], clientOptions);
            var processor = client.CreateProcessor(_configuration["ServiceBus:QueueName"], new ServiceBusProcessorOptions());
            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;
            await processor.StartProcessingAsync(stoppingToken);
        }

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

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception.ToString());
            return Task.CompletedTask;
        }

    }
}
