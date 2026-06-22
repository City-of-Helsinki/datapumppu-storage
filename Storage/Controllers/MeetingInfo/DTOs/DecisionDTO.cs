namespace Storage.Controllers.MeetingInfo.DTOs
{
    /// <summary>
    /// Represents a decision made during a meeting, including its associated documents and metadata.
    /// </summary>
    public class DecisionDTO
    {
        /// <summary>
        /// Gets or sets the native identifier from the source system.
        /// </summary>
        public string NativeId { get; set; }

        /// <summary>
        /// Gets or sets the title or subject of the decision.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the human-readable case identifier label.
        /// </summary>
        public string? CaseIDLabel { get; set; }

        /// <summary>
        /// Gets or sets the unique case identifier.
        /// </summary>
        public string? CaseID { get; set; }

        /// <summary>
        /// Gets or sets the section or chapter reference within the meeting.
        /// </summary>
        public string? Section { get; set; }

        /// <summary>
        /// Gets or sets the HTML content of the decision.
        /// </summary>
        public string? HTML { get; set; }

        /// <summary>
        /// Gets or sets the motion or proposal text for this decision.
        /// </summary>
        public string? Motion { get; set; }

        /// <summary>
        /// Gets or sets the classification code categorizing this decision.
        /// </summary>
        public string? ClassificationCode { get; set; }

        /// <summary>
        /// Gets or sets the title or description of the classification.
        /// </summary>
        public string? ClassificationTitle { get; set; }

        /// <summary>
        /// Gets or sets the language code (e.g., 'fi' for Finnish, 'sv' for Swedish) of the decision content.
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Gets or sets the PDF attachment containing the decision document.
        /// </summary>
        public AttachmentDTO? Pdf { get; set; }

        /// <summary>
        /// Gets or sets the PDF attachment containing the decision history.
        /// </summary>
        public AttachmentDTO? DecisionHistoryPdf { get; set; }

        /// <summary>
        /// Gets or sets the HTML content of the decision history.
        /// </summary>
        public string? DecisionHistoryHtml { get; set; }

        /// <summary>
        /// Gets or sets the list of additional attachments related to this decision.
        /// </summary>
        public List<AttachmentDTO>? Attachments { get; set; }
    }
}