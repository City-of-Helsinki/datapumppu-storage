namespace Storage.Controllers.MeetingInfo.DTOs
{
    /// <summary>
    /// Data transfer object representing a voting event with aggregate results and individual votes.
    /// Includes bilingual titles and vote counts for different vote types.
    /// </summary>
    public class WebApiVotingDTO
    {
        /// <summary>
        /// Gets or sets the title of the "for" option in Finnish.
        /// </summary>
        public string? ForTitleFI { get; set; }
        
        /// <summary>
        /// Gets or sets the title of the "for" option in Swedish.
        /// </summary>
        public string? ForTitleSV { get; set; }

        /// <summary>
        /// Gets or sets the title of the "against" option in Finnish.
        /// </summary>
        public string? AgainstTitleFI { get; set; }

        /// <summary>
        /// Gets or sets the title of the "against" option in Swedish.
        /// </summary>
        public string? AgainstTitleSV { get; set; }

        /// <summary>
        /// Gets or sets the descriptive text of the "for" option in Finnish.
        /// </summary>
        public string? ForTextFI { get; set; }

        /// <summary>
        /// Gets or sets the descriptive text of the "for" option in Swedish.
        /// </summary>
        public string? ForTextSV { get; set; }

        /// <summary>
        /// Gets or sets the descriptive text of the "against" option in Finnish.
        /// </summary>
        public string? AgainstTextFI { get; set; }

        /// <summary>
        /// Gets or sets the descriptive text of the "against" option in Swedish.
        /// </summary>
        public string? AgainstTextSV { get; set; }

        /// <summary>
        /// Gets or sets the total number of votes cast in favor.
        /// </summary>
        public int ForCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of votes cast against.
        /// </summary>
        public int AgainstCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of empty (blank) votes.
        /// </summary>
        public int EmptyCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of absent participants.
        /// </summary>
        public int AbsentCount { get; set; }

        /// <summary>
        /// Gets or sets the array of individual votes with person names and vote types.
        /// </summary>
        public WebApiVoteDTO[] Votes { get; set; }
    }
}