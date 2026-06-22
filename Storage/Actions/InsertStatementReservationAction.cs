using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    /// <summary>
    /// Handles StatementReservation events when a participant requests to make a statement during a meeting.
    /// Manages the queue of statement reservations for orderly discussion.
    /// </summary>
    public class InsertStatementReservationAction : IEventAction
    {
        /// <summary>
        /// Gets the event types handled by this action. Only processes StatementReservation events.
        /// </summary>
        public List<EventType> EventTypes { get; } = new()
            { EventType.StatementReservation };

        private readonly IStatementsRepository _statementsRepository;

        /// <summary>
        /// Initializes a new instance of the InsertStatementReservationAction with the required repository.
        /// </summary>
        /// <param name="statementsRepository">Repository for persisting statement reservation records.</param>
        public InsertStatementReservationAction(IStatementsRepository statementsRepository)
        {
            _statementsRepository = statementsRepository;
        }

        /// <summary>
        /// Executes the action to insert a statement reservation record.
        /// Stores a participant's request to make a statement.
        /// </summary>
        /// <param name="eventBody">The binary event payload containing statement reservation data.</param>
        /// <param name="eventId">The unique identifier for this event.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The database transaction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var statementReservationDto = eventBody.ToObjectFromJson<StatementReservationEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<StatementReservationEventDTO, StatementReservation>()
                    .ForMember(dest => dest.Active, opt => opt.Ignore())
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var statementReservation = mapper.Map<StatementReservation>(statementReservationDto);

            return _statementsRepository.InsertStatementReservation(statementReservation, connection, transaction);
        }
    }
}