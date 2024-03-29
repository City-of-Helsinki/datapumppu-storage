﻿using System.Diagnostics.CodeAnalysis;

namespace Storage.Repositories.Models
{
    public class AgendaItem
    {
        public string MeetingID { get; set; }

        public int AgendaPoint { get; set; }

        public string? Section { get; set; }

        public string? Title { get; set; }

        public string? CaseIDLabel { get; set; }

        public string EditorUserName { get; set; } = string.Empty;

        public string? Html { get; set; }

        public string? Language { get; set; }

        public string? DecisionHistoryHtml { get; set; }

        public DateTime? Timestamp { get; set; }

        public int VideoPosition { get; set; }

        public string ItemTextFi { get; set; } = string.Empty;

        public string ItemNumber { get; set; } = "0";
    }
}

