using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    public class SpeechTimerEventDTO: EventDTO
    {
        public string SeatID { get; set; }

        public string PersonFI { get; set; }

        public string PersonSV { get; set; }

        public int DurationSeconds { get; set; }

        public int SpeechTimer { get; set; }

        public string Direction { get; set; }
    }
}