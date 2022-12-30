namespace Storage.Repositories.Models
{
    public class Vote
    {
        public string MeetingID { get; set; }

        public int VotingNumber { get; set; }

        public string VoterName { get; set; }

        public VoteType VoteType { get; set; }
    }
}
