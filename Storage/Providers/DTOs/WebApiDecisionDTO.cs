using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Repositories.Models;

namespace Storage.Providers.DTOs
{
    /// <summary>
    /// Data transfer object representing a decision made in a meeting.
    /// Includes case identification, decision content, classification, and associated documents.
    /// </summary>
    public class WebApiDecisionDTO
    {
        /// <summary>
        /// Gets or sets the unique meeting identifier.
        /// </summary>
        public string MeetingID { get; set; }

        /// <summary>
        /// Gets or sets the native (original system) identifier for the decision.
        /// </summary>
        public string NativeId { get; set; }

        /// <summary>
        /// Gets or sets the title of the decision.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the case identifier label (e.g., "029-2023-1234").
        /// </summary>
        public string? CaseIDLabel { get; set; }

        /// <summary>
        /// Gets or sets the case identifier.
        /// </summary>
        public string? CaseID { get; set; }

        /// <summary>
        /// Gets or sets the section identifier of the decision.
        /// </summary>
        public string? Section { get; set; }

        /// <summary>
        /// Gets or sets the HTML content of the decision.
        /// </summary>
        public string? Html { get; set; }

        /// <summary>
        /// Gets or sets the HTML content of the decision history.
        /// </summary>
        public string? DecisionHistoryHtml { get; set; }

        /// <summary>
        /// Gets or sets the motion text associated with the decision.
        /// </summary>
        public string? Motion { get; set; }

        /// <summary>
        /// Gets or sets the classification code for the decision.
        /// </summary>
        public string? ClassificationCode { get; set; }

        /// <summary>
        /// Gets or sets the classification title for the decision.
        /// </summary>
        public string? ClassificationTitle { get; set; }

        /// <summary>
        /// Gets or sets the PDF attachment of the decision document.
        /// </summary>
        public WebApiAttachmentDTO Pdf { get; set; }

        /// <summary>
        /// Gets or sets the PDF attachment of the decision history document.
        /// </summary>
        public WebApiAttachmentDTO DecisionHistoryPdf { get; set; }

        /// <summary>
        /// Gets or sets the list of additional attachments associated with the decision.
        /// </summary>
        public List<WebApiAttachmentDTO> Attachments { get; set; }
    }
}
