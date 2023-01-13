namespace Storage.Repositories.Models
{
    public class Case
    {
        public string MeetingID { get; set; }

        public string CaseNumber { get; set; }

        public string ItemNumber { get; set; }

        public Guid EventID { get; set; }

        public string? PropositionFI { get; set; }

        public string? PropositionSV { get; set; }

        public string? CaseTextFI { get; set; }

        public string? CaseTextSV { get; set; }

        public string? ItemTextFI { get; set; }

        public string? ItemTextSV { get; set; }

        public string? Identifier { get; set; }
    }
}
