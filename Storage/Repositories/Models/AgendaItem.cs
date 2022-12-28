using System.Diagnostics.CodeAnalysis;

namespace Storage.Repositories.Models
{
    public class AgendaItem
    {
        public string MeetingID { get; set; }

        public int AgendaPoint { get; set; }

        public string? Section { get; set; }

        public string? Title { get; set; }

        public string? CaseIDLabel { get; set; }

        public string? Html { get; set; }

        public string? Language { get; set; }

        public string? DecisionHistoryHtml { get; set; }
    }
}

