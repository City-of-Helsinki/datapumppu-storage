namespace Storage.Controllers.MeetingInfo.DTOs
{
    /// <summary>
    /// Data transfer object representing an agenda item within a meeting.
    /// Includes item identification, content HTML, video position, and attachments.
    /// </summary>
    public class WebApiAgendaItemDTO
    {
        /// <summary>
        /// Gets or sets the agenda point number.
        /// </summary>
        public int AgendaPoint { get; set; }

        /// <summary>
        /// Gets or sets the section identifier within the agenda.
        /// </summary>
        public string? Section { get; set; }

        /// <summary>
        /// Gets or sets the title of the agenda item.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the case identifier label (e.g., "029-2023-1234").
        /// </summary>
        public string? CaseIDLabel { get; set; }

        /// <summary>
        /// Gets or sets the list of attachments associated with this agenda item.
        /// </summary>
        public List<WebApiAttachmentDTO> Attachments { get; set; } = new List<WebApiAttachmentDTO>();

        /// <summary>
        /// Gets or sets the HTML content of the agenda item.
        /// </summary>
        public string? Html { get; set; }

        /// <summary>
        /// Gets or sets the HTML content of the decision history.
        /// </summary>
        public string? DecisionHistoryHTML { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the agenda item was handled.
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the video position in seconds for video synchronization.
        /// </summary>
        public int? VideoPosition { get; set; }

        /// <summary>
        /// Gets or sets the item number (sub-item identifier within the agenda point).
        /// </summary>
        public string ItemNumber { get; set; } = "0";
    }
}