using System.Diagnostics.CodeAnalysis;

namespace Storage.Repositories.Models
{
    public class AgendaSubItem
    {
        public string ItemTextFi { get; set; } = string.Empty;

        public string ItemNumber { get; set; } = "0";

        public int AgendaPoint { get; set; }
    }
}

