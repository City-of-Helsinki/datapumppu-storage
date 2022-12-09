namespace Storage.Repositories.Models
{
    public class VotingEvent
    {
        public string MeetingID { get; set; }

        public Guid EventID { get; set; }

        public int VotingID { get; set; }

        public DateTime Timestamp { get; set; }

        public VotingType VotingType { get; set; }

        public string? VotingTypeText { get; set; }

        public string ForText { get; set; }

        public string ForTitle { get; set; }

        public string AgainstText { get; set; }

        public string AgainstTitle { get; set; }

        public int? VotesFor { get; set; }

        public int? VotesAgainst { get; set; }

        public int? VotesEmpty { get; set; }

        public int? VotesAbsent { get; set; }

        public List<Vote>? Votes { get; set; }
    }
}
