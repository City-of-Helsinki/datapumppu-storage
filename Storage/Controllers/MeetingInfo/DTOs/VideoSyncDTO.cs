namespace Storage.Controllers.MeetingInfo.DTOs
{
    /// <summary>
    /// Represents a synchronization point linking a meeting timestamp to a video playback position.
    /// </summary>
    public class VideoSyncDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier of the meeting being synchronized.
        /// </summary>
        public string? MeetingID { get; set; }

        /// <summary>
        /// Gets or sets the timestamp within the meeting that corresponds to the video position.
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the position in the video (in seconds or milliseconds) that corresponds to the meeting timestamp.
        /// </summary>
        public int? VideoPosition { get; set; }
    }
}
