namespace Storage.Controllers.Event.DTOs
{
    /// <summary>
    /// Base data transfer object for all meeting events received from Kafka or Azure Service Bus.
    /// Contains common properties shared across all event types.
    /// </summary>
    public class EventDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier of the meeting this event belongs to.
        /// </summary>
        public string MeetingID { get; set; }

        /// <summary>
        /// Gets or sets the type of event, determining which action handlers will process it.
        /// </summary>
        public EventType EventType { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the event occurred.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the sequence number for ordering events within a meeting.
        /// </summary>
        public long SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the case number this event relates to, if applicable.
        /// </summary>
        public string CaseNumber { get; set; }

        /// <summary>
        /// Gets or sets the agenda item number this event relates to, if applicable.
        /// </summary>
        public string ItemNumber { get; set; }

        /// <summary>
        /// Gets or sets the Finnish title of the meeting.
        /// </summary>
        public string MeetingTitleFI { get; set; }

        /// <summary>
        /// Gets or sets the Swedish title of the meeting.
        /// </summary>
        public string MeetingTitleSV { get; set; }
    }
}
