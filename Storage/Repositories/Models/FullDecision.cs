namespace Storage.Repositories.Models
{
    public class FullDecision
    {
        public Decision Decision { get; set; }

        public Attachment Pdf { get; set; }

        public Attachment? DecisionHistoryPdf { get; set; }

        public List<Attachment> Attachments { get; set; }
    }
}
