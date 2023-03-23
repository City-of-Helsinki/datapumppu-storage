namespace Storage.Repositories.Models
{
    public class StartedStatement
    {
        public string MeetingID { get; set; }

        public Guid EventID { get; set; }

        public DateTime Timestamp { get; set; }

        public string Person { get; set; }

        public int SpeakingTime { get; set; }

        public int SpeechTimer { get; set; }

        public DateTime StartTime { get; set; }

        public string Direction { get; set; }

        public string SeatID { get; set; }

        public SpeechType SpeechType { get; set; }

        public string ItemNumber { get; set; } = "0";

        public string? AdditionalInfoFI { get; set; }

        public string? AdditionalInfoSV { get; set; }
    }
}