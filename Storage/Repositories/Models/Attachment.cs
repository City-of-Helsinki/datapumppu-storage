namespace Storage.Repositories.Models
{
    public class Attachment
    {
        public string? MeetingID { get; set; }

        public int? AgendaPoint { get; set; }

        public string? NativeId { get; set; }

        public string? DecisionId { get; set; }

        public string? Title { get; set; }

        public string? AttachmentNumber { get; set; }

        public string? PublicityClass { get; set; }

        public string? SecurityReasons { get; set; }

        public string? Type { get; set; }

        public string? FileURI { get; set; }

        public string? Language { get; set; }

        public string? PersonalData { get; set; }

        public string? Issued { get; set; }
    }
}
