namespace Storage.Controllers.Event.DTOs
{
    public class EventDTO
    {
        public string MeetingID { get; set; }

        public EventType EventType { get; set; }

        public DateTime Timestamp { get; set; }

        public long SequenceNumber { get; set; }

        public string CaseNumber { get; set; }

        public string ItemNumber { get; set; }

        public string MeetingTitleFI { get; set; }

        public string MeetingTitleSV { get; set; }
    }
}
