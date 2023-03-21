using System.Diagnostics.CodeAnalysis;

namespace Storage.Repositories.Models
{
    public class AgendaSubItem
    {
        public int AgendaPoint { get; set; }

        public string ItemTextFi { get; set; } = string.Empty;

        public string ItemNumber { get; set; } = "0";
    }
}

