using Storage.Controllers.Event.DTOs;

namespace Storage.Events.DTOs
{
    public class CaseEventDTO: EventDTO
    {
        public string PropositionFI { get; set; }

        public string PropositionSV { get; set; }

        public string CaseText { get; set; }

        public string ItemText { get; set; }

        public string CaseID { get; set; }
    }
}
