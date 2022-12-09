using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    public class SpeakingTurnsEventDTO: EventDTO
    {
        public List<SpeakingTurnDTO> SpeakingTurns { get; set; }
    }
}
