namespace Storage.Repositories.Models
{
    public class MeetingSeatUpdate
    {
        public string MeetingID { get; set; }

        public Guid EventID { get; set; }

        public long SequenceNumber { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
