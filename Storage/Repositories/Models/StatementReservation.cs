namespace Storage.Repositories.Models
{
    public class StatementReservation
    {
        public string MeetingID { get; set; }

        public Guid EventID { get; set; }

        public DateTime Timestamp { get; set; }

        public string? PersonFI { get; set; }

        public string? PersonSV { get; set; }

        public int? Ordinal { get; set; }

        public string? SeatID { get; set; }
    }
}
