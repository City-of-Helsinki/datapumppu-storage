using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    public class RollCallEventDTO: EventDTO
    {
        public int? Present { get; set; }

        public int? Absent { get; set; }
    }
}
