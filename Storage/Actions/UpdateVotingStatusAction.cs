using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    /// <summary>
    /// Handles VotingStarted and VotingEnded events that track the voting lifecycle during meetings.
    /// Manages voting session status and captures final vote results.
    /// </summary>
    public class UpdateVotingStatusAction : IEventAction
    {
        /// <summary>
        /// Gets the event types handled by this action. Processes VotingStarted and VotingEnded events.
        /// </summary>
        public List<EventType> EventTypes { get; } = new()
            { EventType.VotingStarted, EventType.VotingEnded };

        private readonly IVotingsRepository _votingsRepository;

        /// <summary>
        /// Initializes a new instance of the UpdateVotingStatusAction with the required repository.
        /// </summary>
        /// <param name="votingsRepository">Repository for managing voting data.</param>
        public UpdateVotingStatusAction(IVotingsRepository votingsRepository)
        {
            _votingsRepository = votingsRepository;
        }

        /// <summary>
        /// Executes the action to update voting status.
        /// For VotingStarted events, records the start of a voting session.
        /// For VotingEnded events, saves the voting results and individual votes.
        /// </summary>
        /// <param name="eventBody">The binary event payload containing voting event data.</param>
        /// <param name="eventId">The unique identifier for this event.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The database transaction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var votingEventDto = eventBody.ToObjectFromJson<VotingEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VotingEventDTO, VotingEvent>()
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(_ => eventId));
                cfg.CreateMap<VoteDTO, Vote>()
                    .ForMember(dest => dest.MeetingID, opt => opt.MapFrom(_ => votingEventDto.MeetingID))
                    .ForMember(dest => dest.VotingNumber, opt => opt.MapFrom(_ => votingEventDto.VotingNumber));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var votingEvent = mapper.Map<VotingEvent>(votingEventDto);

            if (votingEventDto.EventType == EventType.VotingStarted)
            {
                return _votingsRepository.UpsertVotingStartedEvent(votingEvent, connection, transaction);
            }
     
            return _votingsRepository.SaveVotingResult(votingEvent, connection, transaction);
        }
    }
}
