using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    public class ReplyReservationEventDTO: EventDTO
    {
        public string? PersonFI { get; set; }

        public string? PersonSV { get; set; }
    }
}
