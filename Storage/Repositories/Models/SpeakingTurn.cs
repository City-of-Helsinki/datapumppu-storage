namespace Storage.Repositories.Models
{
    public class SpeakingTurn
    {
        public string MeetingID { get; set; }

        public Guid EventID { get; set; }

        public string? PersonFI { get; set; }

        public string? PersonSV { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public SpeechType SpeechType { get; set; }

        public int? Duration { get; set; }
    }
}
