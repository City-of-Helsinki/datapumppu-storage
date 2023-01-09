namespace Storage.Repositories.Models
{
    public class DecisionAttachmentData
    {
        public List<Attachment> Existing { get; set; }

        public List<Attachment> NonExisting { get; set; }
    }
}
