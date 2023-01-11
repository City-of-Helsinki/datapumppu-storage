namespace Storage.Controllers.MeetingInfo.DTOs
{
    public class VideoSyncDTO
    {
        public string? MeetingID { get; set; }

        public DateTime? Timestamp { get; set; }

        public int? VideoPosition { get; set; }
    }
}
