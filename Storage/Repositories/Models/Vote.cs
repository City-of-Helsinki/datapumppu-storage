namespace Storage.Repositories.Models
{
    public class Vote
    {
        public int VotingID { get; set; }

        public string VoterName { get; set; }

        public VoteType VoteType { get; set; }
    }
}
