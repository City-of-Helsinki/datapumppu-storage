using Storage.Controllers.Event.DTOs;
namespace Storage.Events.DTOs
{
    public class StatementStartedEventDTO: EventDTO
    {
        public string? Person { get; set; }

        public int? SpeakingTime { get; set; }

        public int? SpeechTimer { get; set; }

        public DateTime? StartTime { get; set; }

        public string? Direction { get; set; }

        public string? SeatID { get; set; }

        public SpeechType? SpeechType { get; set; }

        public string? AdditionalInfoFI { get; set; }

        public string? AdditionalInfoSV { get; set; }
    }
}
