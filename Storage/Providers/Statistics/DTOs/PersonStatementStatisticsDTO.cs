namespace Storage.Providers.Statistics.DTOs
{
    /// <summary>
    /// Data transfer object representing individual statement statistics by person and meeting.
    /// Contains detailed timing information for each statement made.
    /// </summary>
    public class PersonStatementStatisticsDTO
    {
        /// <summary>
        /// Gets or sets the name of the person who made the statement.
        /// </summary>
        public string Person { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unique meeting identifier.
        /// </summary>
        public string MeetingId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the title of the case or agenda item.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the statement started.
        /// </summary>
        public DateTime Started { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the statement ended.
        /// </summary>
        public DateTime Ended { get; set; }

        /// <summary>
        /// Gets or sets the duration of the statement in seconds.
        /// </summary>
        public int DurationSeconds { get; set; }
    }
}
