namespace Storage.Controllers.MeetingInfo.DTOs
{
    public class MeetingDTO
    {
        public DateTime? MeetingDate { get; set; }

        public string? MeetingID { get; set; }

        public string? Name { get; set; }

        public string? Location { get; set; }

        public int? MeetingSequenceNumber { get; set; }

        public List<AgendaItemDTO>? Agendas { get; set; }

        public List<DecisionDTO>? Decisions { get; set; }
    }
}
