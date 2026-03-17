using Microsoft.AspNetCore.Authentication;
using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    /// <summary>
    /// Data transfer object for Propositions events containing voting propositions for a meeting.
    /// Includes all proposals that will be voted on.
    /// </summary>
    public class PropositionsEventDTO: EventDTO
    {
        /// <summary>
        /// Gets or sets the list of propositions for this event.
        /// </summary>
        public List<PropositionDTO> Propositions { get; set; }
    }
}
