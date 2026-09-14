using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    /// <summary>
    /// Handles SpeechTimer events that track speech timing and duration management during meetings.
    /// Records speech timer start, stop, and update events for monitoring speaking time limits.
    /// </summary>
    public class InsertSpeechTimerEventAction : IEventAction
    {
        /// <summary>
        /// Gets the event types handled by this action. Only processes SpeechTimer events.
        /// </summary>
        public List<EventType> EventTypes { get; } = new()
            { EventType.SpeechTimer };

        private readonly ISpeechTimerEventsRepository _speechTimerEventsRepository;

        /// <summary>
        /// Initializes a new instance of the InsertSpeechTimerEventAction with the required repository.
        /// </summary>
        /// <param name="speechTimerEventsRepository">Repository for persisting speech timer event records.</param>
        public InsertSpeechTimerEventAction(ISpeechTimerEventsRepository speechTimerEventsRepository)
        {
            _speechTimerEventsRepository = speechTimerEventsRepository;
        }

        /// <summary>
        /// Executes the action to insert a speech timer event record.
        /// Captures speech timer state changes for managing and enforcing speaking time limits.
        /// </summary>
        /// <param name="eventBody">The binary event payload containing speech timer event data.</param>
        /// <param name="eventId">The unique identifier for this event.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The database transaction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var speechTimerEventDto = eventBody.ToObjectFromJson<SpeechTimerEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SpeechTimerEventDTO, SpeechTimerEvent>()
                    .ForMember(dest => dest.MeetingID, opt => opt.MapFrom(x => speechTimerEventDto.MeetingID))
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var speechTimerEvent = mapper.Map<SpeechTimerEvent>(speechTimerEventDto);

            return _speechTimerEventsRepository.InsertSpeechTimerEvent(speechTimerEvent, connection, transaction);
        }
    }
}
