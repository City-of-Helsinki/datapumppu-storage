namespace Storage.Repositories.Models
{
    public class VideoSync
    {
        public string? MeetingID { get; set; }

        public DateTime? Timestamp { get; set; }

        public int? VideoPosition { get; set; }
    }
}
