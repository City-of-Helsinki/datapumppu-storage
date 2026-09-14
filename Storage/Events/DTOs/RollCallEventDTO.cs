using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    /// <summary>
    /// Data transfer object for RollCallStarted and RollCallEnded events.
    /// Contains attendance counts from the roll call process.
    /// </summary>
    public class RollCallEventDTO: EventDTO
    {
        /// <summary>
        /// Gets or sets the count of participants present at the roll call.
        /// </summary>
        public int? Present { get; set; }

        /// <summary>
        /// Gets or sets the count of participants absent from the roll call.
        /// </summary>
        public int? Absent { get; set; }
    }
}
