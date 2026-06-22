using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    /// <summary>
    /// Data transfer object for VotingStarted and VotingEnded events.
    /// Contains comprehensive voting information including vote counts, propositions, and individual votes.
    /// </summary>
    public class VotingEventDTO: EventDTO
    {
        /// <summary>
        /// Gets or sets the voting number for ordering multiple votes in a meeting.
        /// </summary>
        public int VotingNumber { get; set; }

        /// <summary>
        /// Gets or sets the type of voting (e.g., open, secret).
        /// </summary>
        public VotingType VotingType { get; set; }

        /// <summary>
        /// Gets or sets the Finnish text description of the voting type.
        /// </summary>
        public string? VotingTypeTextFI { get; set; }

        /// <summary>
        /// Gets or sets the Swedish text description of the voting type.
        /// </summary>
        public string? VotingTypeTextSV { get; set; }

        /// <summary>
        /// Gets or sets the Finnish text for the "for" option.
        /// </summary>
        public string ForTextFI { get; set; }

        /// <summary>
        /// Gets or sets the Swedish text for the "for" option.
        /// </summary>
        public string ForTextSV { get; set; }

        /// <summary>
        /// Gets or sets the Finnish title for the "for" option.
        /// </summary>
        public string ForTitleFI { get; set; }

        /// <summary>
        /// Gets or sets the Swedish title for the "for" option.
        /// </summary>
        public string ForTitleSV { get; set; }

        /// <summary>
        /// Gets or sets the Finnish text for the "against" option.
        /// </summary>
        public string AgainstTextFI { get; set; }

        /// <summary>
        /// Gets or sets the Swedish text for the "against" option.
        /// </summary>
        public string AgainstTextSV { get; set; }

        /// <summary>
        /// Gets or sets the Finnish title for the "against" option.
        /// </summary>
        public string AgainstTitleFI { get; set; }

        /// <summary>
        /// Gets or sets the Swedish title for the "against" option.
        /// </summary>
        public string AgainstTitleSV { get; set; }

        /// <summary>
        /// Gets or sets the total count of votes in favor.
        /// </summary>
        public int? VotesFor { get; set; }

        /// <summary>
        /// Gets or sets the total count of votes against.
        /// </summary>
        public int? VotesAgainst { get; set; }

        /// <summary>
        /// Gets or sets the count of empty/abstaining votes.
        /// </summary>
        public int? VotesEmpty { get; set; }

        /// <summary>
        /// Gets or sets the count of absent participants who didn't vote.
        /// </summary>
        public int? VotesAbsent { get; set; }

        /// <summary>
        /// Gets or sets the list of individual votes by participants.
        /// </summary>
        public List<VoteDTO>? Votes { get; set; }
    }
}
