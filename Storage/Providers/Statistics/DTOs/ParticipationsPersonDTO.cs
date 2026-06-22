namespace Storage.Providers.Statistics.DTOs
{
    /// <summary>
    /// Data transfer object representing participation statistics for a person.
    /// Contains a list of meetings and agenda points the person participated in.
    /// </summary>
    public class ParticipationsPersonDTO
    {
        /// <summary>
        /// Gets or sets the name of the person.
        /// </summary>
        public string Person { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of meetings with their attended agenda points.
        /// </summary>
        public List<ParticipationsMeetingDTO> Meetings { get; set; } = new List<ParticipationsMeetingDTO>();
    }
}
