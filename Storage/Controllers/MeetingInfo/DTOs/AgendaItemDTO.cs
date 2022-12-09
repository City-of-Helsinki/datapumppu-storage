namespace Storage.Controllers.MeetingInfo.DTOs
{
    public class AgendaItemDTO
    {
        public int AgendaPoint { get; set; }

        public string? Section { get; set; }

        public string? Title { get; set; }

        public string? CaseIDLabel { get; set; }

        public string? Html { get; set; }

        public string? DecisionHistoryHTML { get; set; }
    }
}