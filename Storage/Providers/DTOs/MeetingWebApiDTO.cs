using Storage.Controllers.MeetingInfo.DTOs;

namespace Storage.Providers.DTOs
{
    public class MeetingWebApiDTO
    {
        public DateTime MeetingDate { get; set; }

        public string MeetingID { get; set; }

        public string Name { get; set; }

        public int MeetingSequenceNumber { get; set; }

        public string Location { get; set; }

        public string? MeetingTitleFI { get; set; }

        public string? MeetingTitleSV { get; set; }

        public DateTime? Started { get; set; }

        public Guid? MeetingStartedEventID { get; set; }

        public DateTime? Ended { get; set; }

        public Guid? MeetingEndedEventID { get; set; }

        public List<AgendaItemDTO>? Agendas { get; set; }

        public List<DecisionWebApiDTO>? Decisions { get; set; }
    }
}
