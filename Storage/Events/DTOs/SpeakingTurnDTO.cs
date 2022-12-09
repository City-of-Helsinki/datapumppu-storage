namespace Storage.Events.DTOs
{
    public class SpeakingTurnDTO
    {
        public string? PersonFI { get; set; }

        public string? PersonSV { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public SpeechType? SpeechType { get; set; }

        public int? Duration { get; set; }
    }
}
