﻿namespace Storage.Controllers.MeetingInfo.DTOs
{
    public class AgendaItemDTO
    {
        public int AgendaPoint { get; set; }

        public string? Section { get; set; }

        public string? Title { get; set; }

        public string? CaseIDLabel { get; set; }

        public string? Html { get; set; }

        public string? Language { get; set; }

        public string? DecisionHistoryHTML { get; set; }

        public AttachmentDTO? Pdf { get; set; }

        public AttachmentDTO? DecisionHistoryPdf { get; set; }

        public AttachmentDTO[] Attachments { get; set; } = new AttachmentDTO[0];
    }
}