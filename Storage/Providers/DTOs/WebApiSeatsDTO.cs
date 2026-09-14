namespace Storage.Controllers.MeetingInfo.DTOs
{
    /// <summary>
    /// Data transfer object representing a participant's seat allocation in a meeting.
    /// Includes person identification and seat positioning information.
    /// </summary>
    public class WebApiSeatDTO
    {
        /// <summary>
        /// Gets or sets the name of the person assigned to the seat.
        /// </summary>
        public string? Person { get; set; }

        /// <summary>
        /// Gets or sets additional information in Finnish (e.g., role or title).
        /// </summary>
        public string? AdditionalInfoFI { get; set; }

        /// <summary>
        /// Gets or sets additional information in Swedish (e.g., role or title).
        /// </summary>
        public string? AdditionalInfoSV { get; set; }

        /// <summary>
        /// Gets or sets the unique seat identifier.
        /// </summary>
        public string? SeatId { get; set; }
    }

    /// <summary>
    /// Data transfer object representing a group of seat allocations active during a specific voting session.
    /// </summary>
    public class WebApiSeatsDTO
    {
        /// <summary>
        /// Chronological voting number this seat allocation is associated with.
        /// </summary>
        public int VotingNumber { get; set; }

        /// <summary>
        /// List of seat allocations active at the start of this voting.
        /// </summary>
        public List<WebApiSeatDTO> Seats { get; set; } = new();
    }
}