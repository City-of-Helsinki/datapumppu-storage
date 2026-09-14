using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    /// <summary>
    /// Handles StatementStarted events that mark the beginning of a participant's statement during a meeting.
    /// Records when statements begin to track speaking order and timing.
    /// </summary>
    public class InsertStartedStatementAction : IEventAction
    {
        /// <summary>
        /// Gets the event types handled by this action. Only processes StatementStarted events.
        /// </summary>
        public List<EventType> EventTypes { get; } = new()
            { EventType.StatementStarted };

        private readonly IStatementsRepository _statementsRepository;

        /// <summary>
        /// Initializes a new instance of the InsertStartedStatementAction with the required repository.
        /// </summary>
        /// <param name="statementsRepository">Repository for persisting started statement records.</param>
        public InsertStartedStatementAction(IStatementsRepository statementsRepository)
        {
            _statementsRepository = statementsRepository;
        }

        /// <summary>
        /// Executes the action to insert a started statement record.
        /// Captures the start of a statement for tracking purposes.
        /// </summary>
        /// <param name="eventBody">The binary event payload containing statement start data.</param>
        /// <param name="eventId">The unique identifier for this event.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The database transaction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var statementStartedDto = eventBody.ToObjectFromJson<StatementStartedEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<StatementStartedEventDTO, StartedStatement>()
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var startedStatement = mapper.Map<StartedStatement>(statementStartedDto);

            return _statementsRepository.InsertStartedStatement(startedStatement, connection, transaction);
        }
    }
}