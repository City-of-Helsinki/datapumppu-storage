using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    public class ReplyReservationEventDTO: EventDTO
    {
        public string? Person { get; set; }

        public string? AdditionalInfoFI { get; set; }

        public string? AdditionalInfoSV { get; set; }
    }
}
