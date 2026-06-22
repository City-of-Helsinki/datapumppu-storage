namespace Storage.Providers.Statistics.DTOs
{
    /// <summary>
    /// Data transfer object representing a person's participation in a specific meeting.
    /// Contains the meeting identifier and the list of agenda points attended.
    /// </summary>
    public class ParticipationsMeetingDTO
    {
        /// <summary>
        /// Gets or sets the unique meeting identifier.
        /// </summary>
        public string MeetingId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of agenda point numbers the person attended.
        /// </summary>
        public List<int> AgendaPoint { get; set; } = new List<int>();
    }
}
