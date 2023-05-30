namespace Storage.Providers.Statistics.DTOs
{
    public class PersonStatementStatisticsDTO
    {
        public string Person { get; set; } = string.Empty;

        public string MeetingId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public DateTime Started { get; set; }

        public DateTime Ended { get; set; }

        public int DurationSeconds { get; set; }
    }
}
