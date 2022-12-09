namespace Storage.Repositories.Models
{
    public class PersonEvent
    {
        public string MeetingID { get; set; }

        public Guid EventID { get; set; }

        public string? PersonFI { get; set; }

        public string? PersonSV { get; set; }

        public EventType? EventType { get; set; }

        public DateTime? Timestamp { get; set; }

        public string? SeatID { get; set; }
    }
}
