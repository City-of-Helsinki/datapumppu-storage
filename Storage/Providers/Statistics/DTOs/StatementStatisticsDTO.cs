namespace Storage.Providers.Statistics.DTOs
{
    /// <summary>
    /// Data transfer object representing statement statistics aggregated by meeting and case.
    /// Contains the number and total duration of statements for a specific case.
    /// </summary>
    public class StatementStatisticsDTO
    {
        /// <summary>
        /// Gets or sets the unique meeting identifier.
        /// </summary>
        public string MeetingId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the case number within the meeting.
        /// </summary>
        public string CaseNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the title of the case or agenda item.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total number of statements made for this case.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the total duration of all statements in seconds.
        /// </summary>
        public int TotalDuration { get; set; }

        /// <summary>
        /// Gets or sets whether this case involves a motion.
        /// </summary>
        public bool IsMotion { get; set; }
    }
}
