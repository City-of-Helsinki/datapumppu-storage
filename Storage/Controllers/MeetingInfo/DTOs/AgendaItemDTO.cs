namespace Storage.Controllers.MeetingInfo.DTOs
{
    /// <summary>
    /// Represents an item on a meeting agenda, including its content and associated documents.
    /// </summary>
    public class AgendaItemDTO
    {
        /// <summary>
        /// Gets or sets the sequential number of this agenda point within the meeting.
        /// </summary>
        public int AgendaPoint { get; set; }

        /// <summary>
        /// Gets or sets the section or chapter reference for this agenda item.
        /// </summary>
        public string? Section { get; set; }

        /// <summary>
        /// Gets or sets the title or subject of the agenda item.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the human-readable case identifier label associated with this agenda item.
        /// </summary>
        public string? CaseIDLabel { get; set; }

        /// <summary>
        /// Gets or sets the HTML content describing the agenda item.
        /// </summary>
        public string? Html { get; set; }

        /// <summary>
        /// Gets or sets the language code (e.g., 'fi' for Finnish, 'sv' for Swedish) of the content.
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Gets or sets the HTML content of the decision history for this agenda item.
        /// </summary>
        public string? DecisionHistoryHTML { get; set; }

        /// <summary>
        /// Gets or sets the PDF attachment containing the agenda item document.
        /// </summary>
        public AttachmentDTO? Pdf { get; set; }

        /// <summary>
        /// Gets or sets the PDF attachment containing the decision history.
        /// </summary>
        public AttachmentDTO? DecisionHistoryPdf { get; set; }

        /// <summary>
        /// Gets or sets the array of additional attachments related to this agenda item.
        /// </summary>
        public AttachmentDTO[] Attachments { get; set; } = new AttachmentDTO[0];
    }
}