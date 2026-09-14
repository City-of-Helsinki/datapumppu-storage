namespace Storage.Controllers.MeetingInfo.DTOs
{
    /// <summary>
    /// Data transfer object representing a sub-item within an agenda point.
    /// Used for organizing nested agenda items.
    /// </summary>
    public class WebApiAgendaSubItemDTO
    {
        /// <summary>
        /// Gets or sets the parent agenda point number.
        /// </summary>
        public int AgendaPoint { get; set; }

        /// <summary>
        /// Gets or sets the Finnish text content of the sub-item.
        /// </summary>
        public string ItemTextFi { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sub-item number (e.g., "1.1", "2.3").
        /// </summary>
        public string ItemNumber { get; set; } = "0";
    }
}
