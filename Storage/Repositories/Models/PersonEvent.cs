namespace Storage.Repositories.Models
{
    public class PersonEvent
    {
        public string MeetingID { get; set; }

        public Guid EventID { get; set; }

        public string? Person { get; set; }

        public EventType? EventType { get; set; }

        public DateTime? Timestamp { get; set; }

        public string? SeatID { get; set; }

        public string? AdditionalInfoFI { get; set; }

        public string? AdditionalInfoSV { get; set; }
    }
}
