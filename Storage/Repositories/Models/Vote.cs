namespace Storage.Repositories.Models
{
    public class Vote
    {
        public string MeetingID { get; set; }

        public int VotingNumber { get; set; }

        public string Person { get; set; }

        public VoteType VoteType { get; set; }

        public string AdditionalInfoFI { get; set; }

        public string AdditionalInfoSV { get; set; }
    }
}
