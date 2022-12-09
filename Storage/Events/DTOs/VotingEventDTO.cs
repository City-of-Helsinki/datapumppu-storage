using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    public class VotingEventDTO: EventDTO
    {
        public int VotingID { get; set; }

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

        public List<VoteDTO>? Votes { get; set; }
    }
}
