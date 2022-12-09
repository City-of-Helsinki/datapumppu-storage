using Microsoft.AspNetCore.Authentication;
using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    public class PropositionsEventDTO: EventDTO
    {
        public List<PropositionDTO> Propositions { get; set; }
    }
}
