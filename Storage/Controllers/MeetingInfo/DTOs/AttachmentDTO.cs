namespace Storage.Controllers.MeetingInfo.DTOs
{
    /// <summary>
    /// Represents a document attachment associated with a meeting, decision, or agenda item.
    /// </summary>
    public class AttachmentDTO
    {
        /// <summary>
        /// Gets or sets the native identifier from the source system.
        /// </summary>
        public string? NativeId { get; set; }

        /// <summary>
        /// Gets or sets the title or name of the attachment.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the sequential attachment number within its parent document.
        /// </summary>
        public string? AttachmentNumber { get; set; }

        /// <summary>
        /// Gets or sets the publicity classification (e.g., public, confidential, secret).
        /// </summary>
        public string? PublicityClass { get; set; }

        /// <summary>
        /// Gets or sets the array of reasons why the document may have restricted access.
        /// </summary>
        public string[]? SecurityReasons { get; set; }

        /// <summary>
        /// Gets or sets the MIME type or file type of the attachment.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the URI or file path to access the attachment.
        /// </summary>
        public string? FileURI { get; set; }

        /// <summary>
        /// Gets or sets the language code (e.g., 'fi' for Finnish, 'sv' for Swedish) of the attachment content.
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Gets or sets an indicator of whether the attachment contains personal data.
        /// </summary>
        public string? PersonalData { get; set; }

        /// <summary>
        /// Gets or sets the date when the attachment was issued or published.
        /// </summary>
        public string? Issued { get; set; }
    }
}
