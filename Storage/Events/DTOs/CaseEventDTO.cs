using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    /// <summary>
    /// Data transfer object for Case events containing agenda case information.
    /// Includes case title, text content, and propositions in both Finnish and Swedish.
    /// </summary>
    public class CaseEventDTO: EventDTO
    {
        /// <summary>
        /// Gets or sets the Finnish proposition text for the case.
        /// </summary>
        public string? PropositionFI { get; set; }

        /// <summary>
        /// Gets or sets the Swedish proposition text for the case.
        /// </summary>
        public string? PropositionSV { get; set; }

        /// <summary>
        /// Gets or sets the Finnish case text content.
        /// </summary>
        public string? CaseTextFI { get; set; }

        /// <summary>
        /// Gets or sets the Swedish case text content.
        /// </summary>
        public string? CaseTextSV { get; set; }

        /// <summary>
        /// Gets or sets the Finnish agenda item text.
        /// </summary>
        public string? ItemTextFI { get; set; }

        /// <summary>
        /// Gets or sets the Swedish agenda item text.
        /// </summary>
        public string? ItemTextSV { get; set; }

        /// <summary>
        /// Gets or sets the case identifier.
        /// </summary>
        public string? Identifier { get; set; }
    }
}
