namespace Storage.Controllers.MeetingInfo.DTOs
{
    /// <summary>
    /// Data transfer object representing a statement made during a meeting.
    /// Includes speaker information, timing details, speech type, and video synchronization data.
    /// </summary>
    public class WebApiStatementsDTO
    {
        /// <summary>
        /// Gets or sets the name of the person making the statement.
        /// </summary>
        public string Person { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the start time of the statement.
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the statement.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the speech type identifier (mapped from SpeechType enum).
        /// </summary>
        public int SpeechType { get; set; }

        /// <summary>
        /// Gets or sets the duration of the statement in seconds.
        /// </summary>
        public int DurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets additional information in Finnish.
        /// </summary>
        public string AdditionalInfoFI { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets additional information in Swedish.
        /// </summary>
        public string AdditionalInfoSV { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the title of the agenda item related to this statement.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the case number within the meeting.
        /// </summary>
        public int? CaseNumber { get; set; }

        /// <summary>
        /// Gets or sets the item number (sub-item identifier within the case).
        /// </summary>
        public string ItemNumber { get; set; } = "0";

        /// <summary>
        /// Gets or sets the unique meeting identifier.
        /// </summary>
        public string? MeetingId { get; set; }

        /// <summary>
        /// Gets or sets the video position in seconds for synchronization.
        /// </summary>
        public int VideoPosition { get; set; }

        /// <summary>
        /// Gets or sets the video link URL for viewing the statement.
        /// </summary>
        public string VideoLink { get; set; } = string.Empty;
    }
}