using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    /// <summary>
    /// Data transfer object for Statements events containing a collection of statement records.
    /// Used to batch-update statement information including timing and participant data.
    /// </summary>
    public class StatementsEventDTO: EventDTO
    {
        /// <summary>
        /// Gets or sets the list of statements for this event.
        /// </summary>
        public List<StatementDTO> Statements { get; set; }
    }
}
