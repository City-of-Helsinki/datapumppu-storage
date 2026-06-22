namespace Storage.Controllers.MeetingInfo.DTOs
{
    /// <summary>
    /// Data transfer object representing an attachment or document associated with an agenda item or decision.
    /// Includes metadata about publicity, security, document type, and file location.
    /// </summary>
    public class WebApiAttachmentDTO
    {
        /// <summary>
        /// Gets or sets the native (original system) identifier for the attachment.
        /// </summary>
        public string? NativeId { get; set; }

        /// <summary>
        /// Gets or sets the title of the attachment.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the attachment number (e.g., "1", "2").
        /// </summary>
        public string? AttachmentNumber { get; set; }

        /// <summary>
        /// Gets or sets the publicity class (e.g., "Public", "Confidential").
        /// </summary>
        public string? PublicityClass { get; set; }

        /// <summary>
        /// Gets or sets the security reasons for restricted access.
        /// </summary>
        public string? SecurityReasons { get; set; }

        /// <summary>
        /// Gets or sets the document type (e.g., "PDF", "Word").
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the file URI or URL for accessing the attachment.
        /// </summary>
        public string? FileURI { get; set; }

        /// <summary>
        /// Gets or sets the language of the attachment content.
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Gets or sets whether the attachment contains personal data.
        /// </summary>
        public string? PersonalData { get; set; }

        /// <summary>
        /// Gets or sets the issue date of the attachment.
        /// </summary>
        public string? Issued { get; set; }
    }
}
