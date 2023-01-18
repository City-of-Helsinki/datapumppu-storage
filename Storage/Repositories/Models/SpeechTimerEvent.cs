namespace Storage.Repositories.Models
{
    public class SpeechTimerEvent
    {
        public string MeetingID { get; set; }

        public Guid EventID { get; set; }

        public string SeatID { get; set; }

        public string Person { get; set; }

        public int DurationSeconds { get; set; }

        public int SpeechTimer { get; set; }

        public string Direction { get; set; }

        public string AdditionalInfoFI { get; set; }

        public string AdditionalInfoSV { get; set; }
    }
}