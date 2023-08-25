namespace Storage.Controllers.MeetingInfo.DTOs
{
    public class AgendaPointEditDTO
    {
        public string MeetingId { get; set; } = string.Empty;

        public int AgendaPoint { get; set; }

        public string EditorUserName { get; set; } = string.Empty;

        public string Html { get; set; } = string.Empty;

        public string Language {  get; set; } = string.Empty;
    }
}
