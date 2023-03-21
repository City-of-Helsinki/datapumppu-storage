namespace Storage.Repositories.Models
{
    public class StatementReservation
    {
        public string MeetingID { get; set; }

        public Guid EventID { get; set; }

        public DateTime Timestamp { get; set; }

        public string? Person { get; set; }

        public int? Ordinal { get; set; }

        public string? SeatID { get; set; }

        public string? AdditionalInfoFI { get; set; }

        public string? AdditionalInfoSV { get; set; }

        public int? CaseNumber { get; set; }

        public string ItemNumber { get; set; } = "0";

        public bool? Active { get; set; }
    }
}
