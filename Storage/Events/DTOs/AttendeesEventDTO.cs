using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    public class AttendeesEventDTO: EventDTO
    {
        public List<MeetingSeatDTO> MeetingSeats { get; set; }
    }
}
