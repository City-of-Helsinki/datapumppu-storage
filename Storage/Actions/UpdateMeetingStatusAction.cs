using System.Data;
using AutoMapper;
using Storage.Controllers.Event.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace Storage.Actions
{
    /// <summary>
    /// Handles MeetingStarted and MeetingEnded events that track the lifecycle of a meeting.
    /// Updates meeting status and timestamps when meetings begin or conclude.
    /// </summary>
    public class UpdateMeetingStatusAction : IEventAction
    {
        /// <summary>
        /// Gets the event types handled by this action. Processes MeetingStarted and MeetingEnded events.
        /// </summary>
        public List<EventType> EventTypes { get; } = new()
            { EventType.MeetingStarted, EventType.MeetingEnded };

        private readonly IMeetingsRepository _meetingsRepository;

        /// <summary>
        /// Initializes a new instance of the UpdateMeetingStatusAction with the required repository.
        /// </summary>
        /// <param name="meetingsRepository">Repository for managing meeting data.</param>
        public UpdateMeetingStatusAction(IMeetingsRepository meetingsRepository)
        {
            _meetingsRepository = meetingsRepository;
        }

        /// <summary>
        /// Executes the action to update meeting status.
        /// For MeetingStarted events, upserts the meeting start time.
        /// For MeetingEnded events, updates the meeting end time.
        /// </summary>
        /// <param name="eventBody">The binary event payload containing meeting status data.</param>
        /// <param name="eventId">The unique identifier for this event.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The database transaction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var meetingStatusEvent = eventBody.ToObjectFromJson<SimpleEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SimpleEventDTO, Meeting>()
                    .ForMember(dest => dest.MeetingStartedEventID, opt =>
                    {
                        opt.PreCondition(src => src.EventType == EventType.MeetingStarted);
                        opt.MapFrom(x => eventId);
                    })
                    .ForMember(dest => dest.MeetingStarted, opt =>
                    {
                        opt.PreCondition(src => src.EventType == EventType.MeetingStarted);
                        opt.MapFrom(src => src.Timestamp);
                    })
                    .ForMember(dest => dest.MeetingEndedEventID, opt =>
                    {
                        opt.PreCondition(src => src.EventType == EventType.MeetingEnded);
                        opt.MapFrom(x => eventId);
                    })
                    .ForMember(dest => dest.MeetingEnded, opt =>
                    {
                         opt.PreCondition(src => src.EventType == EventType.MeetingEnded);
                         opt.MapFrom(src => src.Timestamp);
                    })
                    .ForMember(dest => dest.MeetingDate, opt => opt.Ignore())
                    .ForMember(dest => dest.Name, opt => opt.Ignore())
                    .ForMember(dest => dest.MeetingSequenceNumber, opt => opt.Ignore())
                    .ForMember(dest => dest.Location, opt => opt.Ignore());
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var meeting = mapper.Map<Meeting>(meetingStatusEvent);

            if (meetingStatusEvent.EventType == EventType.MeetingStarted)
            {
                return _meetingsRepository.UpsertMeetingStartTime(meeting, connection, transaction);
            }
            return _meetingsRepository.UpdateMeetingEndTime(meeting, connection, transaction);
        }
    }
}
