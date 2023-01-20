namespace Storage.Controllers.MeetingInfo.DTOs
{
    public class WebApiAgendaItemDTO
    {
        public int AgendaPoint { get; set; }

        public string? Section { get; set; }

        public string? Title { get; set; }

        public string? CaseIDLabel { get; set; }

        public List<WebApiAttachmentDTO> Attachments { get; set; } = new List<WebApiAttachmentDTO>();

        public string? Html { get; set; }

        public string? DecisionHistoryHTML { get; set; }

        public DateTime? Timestamp { get; set; }

        public int? VideoPosition { get; set; }
    }
}