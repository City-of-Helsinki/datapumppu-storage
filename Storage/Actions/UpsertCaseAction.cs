using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    /// <summary>
    /// Handles Case events that contain information about meeting cases or agenda items.
    /// Updates or inserts case records with title, description, and metadata.
    /// </summary>
    public class UpsertCaseAction : IEventAction
    {
        /// <summary>
        /// Gets the event types handled by this action. Only processes Case events.
        /// </summary>
        public List<EventType> EventTypes { get; } = new()
            { EventType.Case };

        private readonly ICaseRepository _caseRepository;

        /// <summary>
        /// Initializes a new instance of the UpsertCaseAction with the required repository.
        /// </summary>
        /// <param name="caseRepository">Repository for managing case data.</param>
        public UpsertCaseAction(ICaseRepository caseRepository)
        {
            _caseRepository = caseRepository;
        }

        /// <summary>
        /// Executes the action to upsert a case record.
        /// Creates a new case or updates an existing one with the provided data.
        /// </summary>
        /// <param name="eventBody">The binary event payload containing case data.</param>
        /// <param name="eventId">The unique identifier for this event.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The database transaction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var caseEventDto = eventBody.ToObjectFromJson<CaseEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CaseEventDTO, Case>()
                    .ForMember(dest => dest.MeetingID, opt => opt.MapFrom(x => caseEventDto.MeetingID))
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var caseItem = mapper.Map<Case>(caseEventDto);

            return _caseRepository.UpsertCase(caseItem, connection, transaction);
        }
    }
}
