namespace Storage.Repositories.Models
{
    public class DecisionAttachmentData
    {
        public List<DecisionAttachment> Existing { get; set; }

        public List<DecisionAttachment> NonExisting { get; set; }
    }
}
