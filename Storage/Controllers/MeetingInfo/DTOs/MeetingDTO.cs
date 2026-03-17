namespace Storage.Controllers.MeetingInfo.DTOs
{
    /// <summary>
    /// Represents a meeting with its associated agendas and decisions.
    /// </summary>
    public class MeetingDTO
    {
        /// <summary>
        /// Gets or sets the date and time when the meeting is scheduled or took place.
        /// </summary>
        public DateTime? MeetingDate { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the meeting.
        /// </summary>
        public string? MeetingID { get; set; }

        /// <summary>
        /// Gets or sets the name or title of the meeting.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the physical or virtual location where the meeting takes place.
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Gets or sets the sequential number of the meeting within its year.
        /// </summary>
        public int? MeetingSequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the list of agenda items for a meeting.
        /// </summary>
        public List<AgendaItemDTO>? Agendas { get; set; }

        /// <summary>
        /// Gets or sets the list of decisions made during a meeting.
        /// </summary>
        public List<DecisionDTO>? Decisions { get; set; }
    }
}
