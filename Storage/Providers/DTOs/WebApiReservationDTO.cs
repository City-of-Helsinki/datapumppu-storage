namespace Storage.Providers.DTOs
{
    /// <summary>
    /// Data transfer object representing a statement or reply reservation.
    /// Tracks who is scheduled or currently speaking during a meeting case.
    /// </summary>
    public class WebApiReservationDTO
    {
        /// <summary>
        /// Gets or sets the unique meeting identifier.
        /// </summary>
        public string MeetingID { get; set; }

        /// <summary>
        /// Gets or sets the agenda point number.
        /// </summary>
        public int AgendaPoint { get; set; }

        /// <summary>
        /// Gets or sets the item number (sub-item identifier within the agenda point).
        /// </summary>
        public string ItemNumber { get; set; } = "0";

        /// <summary>
        /// Gets or sets the timestamp when the reservation was made.
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the name of the person who made the reservation.
        /// </summary>
        public string? Person { get; set; }

        /// <summary>
        /// Gets or sets the ordinal position in the reservation queue.
        /// </summary>
        public int? Ordinal { get; set; }

        /// <summary>
        /// Gets or sets the seat identifier of the person.
        /// </summary>
        public string? SeatID { get; set; }

        /// <summary>
        /// Gets or sets additional information in Finnish.
        /// </summary>
        public string? AdditionalInfoFI { get; set; }

        /// <summary>
        /// Gets or sets additional information in Swedish.
        /// </summary>
        public string? AdditionalInfoSV { get; set; }

        /// <summary>
        /// Gets or sets whether this reservation is currently active (person is speaking).
        /// </summary>
        public bool? Active { get; set; }
    }
}
