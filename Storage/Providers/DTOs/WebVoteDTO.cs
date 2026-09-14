namespace Storage.Controllers.MeetingInfo.DTOs
{
    /// <summary>
    /// Data transfer object representing an individual vote cast by a person.
    /// </summary>
    public class WebApiVoteDTO
    {
        /// <summary>
        /// Gets or sets the name of the person who cast the vote.
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// Gets or sets the vote type identifier (mapped from VoteType enum).
        /// </summary>
        public int VoteType { get; set; }
    }
}