using System.Data;
using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace Storage.Actions
{
    /// <summary>
    /// Handles Attendees events that update the current seating arrangement in a meeting.
    /// Tracks seat allocations and updates for meeting participants.
    /// </summary>
    public class UpdateMeetingSeatsAction : IEventAction
    {
        /// <summary>
        /// Gets the event types handled by this action. Only processes Attendees events.
        /// </summary>
        public List<EventType> EventTypes { get; } = new()
            { EventType.Attendees };

        private readonly IMeetingSeatsRepository _meetingSeatsRepository;

        /// <summary>
        /// Initializes a new instance of the UpdateMeetingSeatsAction with the required repository.
        /// </summary>
        /// <param name="meetingSeatsRepository">Repository for managing meeting seat data.</param>
        public UpdateMeetingSeatsAction(IMeetingSeatsRepository meetingSeatsRepository)
        {
            _meetingSeatsRepository = meetingSeatsRepository;
        }

        /// <summary>
        /// Executes the action to update meeting seat allocations.
        /// Processes the seat arrangement data and updates the database with current seating information.
        /// </summary>
        /// <param name="eventBody">The binary event payload containing attendee and seat data.</param>
        /// <param name="eventId">The unique identifier for this event.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The database transaction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var attendeesEventDto = eventBody.ToObjectFromJson<AttendeesEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AttendeesEventDTO, MeetingSeatUpdate>()
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
                cfg.CreateMap<MeetingSeatDTO, MeetingSeat>();
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var meetingSeats = attendeesEventDto.MeetingSeats.Select(meetingSeatDto => mapper.Map<MeetingSeat>(meetingSeatDto)).ToList();
            var meetingSeatUpdate = mapper.Map<MeetingSeatUpdate>(attendeesEventDto);

            return _meetingSeatsRepository.InsertMeetingSeatUpdate(meetingSeatUpdate, meetingSeats, connection, transaction);
        }
    }
}
