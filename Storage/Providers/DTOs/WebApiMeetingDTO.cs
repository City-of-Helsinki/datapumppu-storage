using Storage.Controllers.MeetingInfo.DTOs;

namespace Storage.Providers.DTOs
{
    /// <summary>
    /// Data transfer object representing a complete meeting with agenda items and decisions.
    /// Includes meeting metadata, timing information, and nested collections of agendas and decisions.
    /// </summary>
    public class WebApiMeetingDTO
    {
        /// <summary>
        /// Gets or sets the date of the meeting.
        /// </summary>
        public DateTime MeetingDate { get; set; }

        /// <summary>
        /// Gets or sets the unique meeting identifier (e.g., "029002023001").
        /// </summary>
        public string MeetingID { get; set; }

        /// <summary>
        /// Gets or sets the name of the meeting.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the meeting sequence number within the year.
        /// </summary>
        public int MeetingSequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the physical location of the meeting.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the meeting title in Finnish.
        /// </summary>
        public string? MeetingTitleFI { get; set; }

        /// <summary>
        /// Gets or sets the meeting title in Swedish.
        /// </summary>
        public string? MeetingTitleSV { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the meeting started.
        /// </summary>
        public DateTime? MeetingStarted { get; set; }

        /// <summary>
        /// Gets or sets the event identifier for the meeting started event.
        /// </summary>
        public Guid? MeetingStartedEventID { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the meeting ended.
        /// </summary>
        public DateTime? MeetingEnded { get; set; }

        /// <summary>
        /// Gets or sets the event identifier for the meeting ended event.
        /// </summary>
        public Guid? MeetingEndedEventID { get; set; }

        /// <summary>
        /// Gets or sets the list of agenda items for this meeting.
        /// </summary>
        public List<WebApiAgendaItemDTO>? Agendas { get; set; }

        /// <summary>
        /// Gets or sets the list of decisions made in this meeting.
        /// </summary>
        public List<WebApiDecisionDTO>? Decisions { get; set; }
    }
}
