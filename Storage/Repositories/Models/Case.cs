namespace Storage.Repositories.Models
{
    public class Case
    {
        public string MeetingID { get; set; }

        public Guid EventID { get; set; }

        public string? PropositionFI { get; set; }

        public string? PropositionSV { get; set; }

        public string? CaseText { get; set; }

        public string? ItemText { get; set; }

        public string? CaseID { get; set; }
    }
}
