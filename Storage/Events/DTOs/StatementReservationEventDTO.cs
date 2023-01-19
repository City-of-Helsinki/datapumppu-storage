using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    public class StatementReservationEventDTO: EventDTO
    {
        public string? PersonFI { get; set; }

        public string? PersonSV { get; set; }

        public int? Ordinal { get; set; }

        public string? SeatID { get; set; }
    }
}
