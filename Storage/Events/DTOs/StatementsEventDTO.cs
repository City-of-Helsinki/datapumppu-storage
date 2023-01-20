using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    public class StatementsEventDTO: EventDTO
    {
        public List<StatementDTO> Statements { get; set; }
    }
}
