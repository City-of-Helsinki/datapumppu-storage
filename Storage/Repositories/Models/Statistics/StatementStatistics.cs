namespace Storage.Repositories.Models.Statistics
{
    public class StatementStatistics
    {
        public string MeetingId { get; set; } = string.Empty;

        public string CaseNumber { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public int Count { get; set; }

        public int TotalDuration { get; set; }

        public bool IsMotion { get; set; }
    }
}
