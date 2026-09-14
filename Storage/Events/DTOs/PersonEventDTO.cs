using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    /// <summary>
    /// Data transfer object for PersonArrived and PersonLeft events.
    /// Tracks participant arrivals and departures during a meeting.
    /// </summary>
    public class PersonEventDTO: EventDTO
    {
        /// <summary>
        /// Gets or sets the name or identifier of the person.
        /// </summary>
        public string? Person { get; set; }

        /// <summary>
        /// Gets or sets the seat identifier where the person is or was seated.
        /// </summary>
        public string? SeatID { get; set; }

        /// <summary>
        /// Gets or sets additional information in Finnish about the person's arrival or departure.
        /// </summary>
        public string? AdditionalInfoFI { get; set; }

        /// <summary>
        /// Gets or sets additional information in Swedish about the person's arrival or departure.
        /// </summary>
        public string? AdditionalInfoSV { get; set; }

    }
}
