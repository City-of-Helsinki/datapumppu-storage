namespace Storage.Repositories.Models
{
    public class Event
    {
        public string MeetingID { get; set; }

        public Guid EventID { get; set; }

        public EventType EventType { get; set; }

        public DateTime Timestamp { get; set; }

        public long SequenceNumber { get; set; }

        public string CaseNumber { get; set; }

        public string ItemNumber { get; set; }
    }
}
