﻿namespace Storage.Controllers.MeetingInfo.DTOs
{
    public class DecisionDTO
    {
        public string NativeId { get; set; }

        public string Title { get; set; }

        public string? CaseIDLabel { get; set; }

        public string? CaseID { get; set; }

        public string? Section { get; set; }

        public string? HTML { get; set; }

        public string? Motion { get; set; }

        public string? ClassificationCode { get; set; }

        public string? ClassificationTitle { get; set; }

        public string? Language { get; set; }

        public AttachmentDTO? Pdf { get; set; }

        public AttachmentDTO? DecisionHistoryPdf { get; set; }

        public string? DecisionHistoryHtml { get; set; }

        public List<AttachmentDTO>? Attachments { get; set; }
    }
}