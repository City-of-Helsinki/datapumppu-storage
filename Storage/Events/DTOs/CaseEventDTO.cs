using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    public class CaseEventDTO: EventDTO
    {
        public string? PropositionFI { get; set; }

        public string? PropositionSV { get; set; }

        public string? CaseTextFI { get; set; }

        public string? CaseTextSV { get; set; }

        public string? ItemTextFI { get; set; }

        public string? ItemTextSV { get; set; }

        public string? Identifier { get; set; }
    }
}
