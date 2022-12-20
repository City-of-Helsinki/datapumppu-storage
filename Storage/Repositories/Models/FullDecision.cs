namespace Storage.Repositories.Models
{
    public class FullDecision
    {
        public Decision Decision { get; set; }

        public DecisionAttachment Pdf { get; set; }

        public DecisionAttachment DecisionHistoryPdf { get; set; }

        public List<DecisionAttachment> Attachments { get; set; }
    }
}
