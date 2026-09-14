using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    /// <summary>
    /// Handles Propositions events that contain voting propositions for a meeting.
    /// Stores the various proposals that will be voted on during the meeting.
    /// </summary>
    public class InsertPropositionsEventAction : IEventAction
    {
        /// <summary>
        /// Gets the event types handled by this action. Only processes Propositions events.
        /// </summary>
        public List<EventType> EventTypes { get; } = new()
            { EventType.Propositions };

        private readonly IPropositionsRepository _propositionsRepository;

        /// <summary>
        /// Initializes a new instance of the InsertPropositionsEventAction with the required repository.
        /// </summary>
        /// <param name="propositionsRepository">Repository for persisting proposition records.</param>
        public InsertPropositionsEventAction(IPropositionsRepository propositionsRepository)
        {
            _propositionsRepository = propositionsRepository;
        }

        /// <summary>
        /// Executes the action to insert proposition records.
        /// Processes multiple propositions from the event and stores them in the database.
        /// </summary>
        /// <param name="eventBody">The binary event payload containing proposition data.</param>
        /// <param name="eventId">The unique identifier for this event.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The database transaction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var propositionEventDto = eventBody.ToObjectFromJson<PropositionsEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PropositionDTO, Proposition>()
                    .ForMember(dest => dest.MeetingID, opt => opt.MapFrom(x => propositionEventDto.MeetingID))
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var propositions = propositionEventDto.Propositions.Select(proposition => mapper.Map<Proposition>(proposition)).ToList();

            return _propositionsRepository.InsertPropositions(propositions, connection, transaction);
        }
    }
}