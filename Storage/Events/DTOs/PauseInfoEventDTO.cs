using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    /// <summary>
    /// Data transfer object for PauseInfo events that indicate meeting pauses or breaks.
    /// Contains information about the pause reason or status.
    /// </summary>
    public class PauseInfoEventDTO: EventDTO
    {
        /// <summary>
        /// Gets or sets the pause information text describing the reason or nature of the pause.
        /// </summary>
        public string Info { get; set; }
    }
}
