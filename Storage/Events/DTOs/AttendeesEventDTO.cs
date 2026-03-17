using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    /// <summary>
    /// Data transfer object for Attendees events containing the current meeting seating arrangement.
    /// Updated when participants' seat assignments change.
    /// </summary>
    public class AttendeesEventDTO: EventDTO
    {
        /// <summary>
        /// Gets or sets the list of meeting seats with assigned participants.
        /// </summary>
        public List<MeetingSeatDTO> MeetingSeats { get; set; }
    }
}
