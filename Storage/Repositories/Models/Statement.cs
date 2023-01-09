namespace Storage.Repositories.Models
{
    public class Statement
    {
        public string MeetingID { get; set; } = string.Empty;

        public Guid EventID { get; set; }

        public string? Person { get; set; }

        public DateTime? Started { get; set; }

        public DateTime? Ended { get; set; }

        public SpeechType SpeechType { get; set; }

        public int? DurationSeconds { get; set; }

        public string? AdditionalInfoFI { get; set; }

        public string? AdditionalInfoSV { get; set; }
    }
}
