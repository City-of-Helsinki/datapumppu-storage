namespace Storage.Controllers.MeetingInfo.DTOs
{
    /// <summary>
    /// Represents an agenda point edit request with editor information and content.
    /// </summary>
    public class AgendaPointEditDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier of the meeting containing the agenda point.
        /// </summary>
        public string MeetingId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the agenda point number within the meeting.
        /// </summary>
        public int AgendaPoint { get; set; }

        /// <summary>
        /// Gets or sets the username of the person who edited the agenda point.
        /// </summary>
        public string EditorUserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the HTML content of the edited agenda point.
        /// </summary>
        public string Html { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the language code (e.g., 'fi' for Finnish, 'sv' for Swedish) of the content.
        /// </summary>
        public string Language {  get; set; } = string.Empty;
    }
}
