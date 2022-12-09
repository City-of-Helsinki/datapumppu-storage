using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    public class BreakNoticeEventDTO: EventDTO
    {
        public string Notice { get; set; }
    }
}
