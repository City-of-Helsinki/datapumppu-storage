using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    /// <summary>
    /// Handles Statements events that contain comprehensive statement data for a meeting.
    /// Updates or inserts statement records with timing, duration, and participant information.
    /// </summary>
    public class UpdateStatementsAction : IEventAction
    {
        /// <summary>
        /// Gets the event types handled by this action. Only processes Statements events.
        /// </summary>
        public List<EventType> EventTypes { get; } = new()
            { EventType.Statements };

        private readonly IStatementsRepository _statementsRepository;

        /// <summary>
        /// Initializes a new instance of the UpdateStatementsAction with the required repository.
        /// </summary>
        /// <param name="statementsRepository">Repository for managing statement data.</param>
        public UpdateStatementsAction(IStatementsRepository statementsRepository)
        {
            _statementsRepository = statementsRepository;
        }

        /// <summary>
        /// Executes the action to update or insert statement records.
        /// Processes multiple statements from the event and upserts them to the database.
        /// </summary>
        /// <param name="eventBody">The binary event payload containing statement data.</param>
        /// <param name="eventId">The unique identifier for this event.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The database transaction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var statementsEventDto = eventBody.ToObjectFromJson<StatementsEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<StatementDTO, Statement>()
                    .ForMember(dest => dest.MeetingID, opt => opt.MapFrom(x => statementsEventDto.MeetingID))
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId))
                    .ForMember(dest => dest.Started, opt => opt.MapFrom(src => src.StartTime))
                    .ForMember(dest => dest.Ended, opt => opt.MapFrom(src => src.EndTime))
                    .ForMember(dest => dest.CaseNumber, opt => opt.Ignore())
                    .ForMember(dest => dest.Title, opt => opt.Ignore())
                    .ForMember(dest => dest.ItemNumber, opt => opt.Ignore())
                    .ForMember(dest => dest.DurationSeconds, opt => opt.MapFrom(src => src.Duration));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var statements = statementsEventDto.Statements.Select(statementDto => mapper.Map<Statement>(statementDto)).ToList();

            return _statementsRepository.UpsertStatements(statements, connection, transaction);
        }
    }
}
