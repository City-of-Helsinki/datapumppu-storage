using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Repositories.Models;

namespace Storage.Providers.DTOs
{
    public class WebApiDecisionDTO
    {
        public string MeetingID { get; set; }

        public string NativeId { get; set; }

        public string? Title { get; set; }

        public string? CaseIDLabel { get; set; }

        public string? CaseID { get; set; }

        public string? Section { get; set; }

        public string? Html { get; set; }

        public string? DecisionHistoryHtml { get; set; }

        public string? Motion { get; set; }

        public string? ClassificationCode { get; set; }

        public string? ClassificationTitle { get; set; }

        public WebApiAttachmentDTO Pdf { get; set; }

        public WebApiAttachmentDTO DecisionHistoryPdf { get; set; }

        public List<WebApiAttachmentDTO> Attachments { get; set; }
    }
}
