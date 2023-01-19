using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    public class StatementReservationEventDTO: EventDTO
    {
        public string? Person { get; set; }

        public int? Ordinal { get; set; }

        public string? SeatID { get; set; }

        public string? AdditionalInfoFI { get; set; }

        public string? AdditionalInfoSV { get; set; }
    }
}
