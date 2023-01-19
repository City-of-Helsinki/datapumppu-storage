using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    public class PauseInfoEventDTO: EventDTO
    {
        public string Info { get; set; }
    }
}
