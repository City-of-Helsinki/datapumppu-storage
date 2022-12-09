namespace Storage.Repositories.Models
{
    public class Proposition
    {
        public string MeetingID { get; set; }

        public Guid EventID { get; set; }

        public string? TextFI { get; set; }

        public string? TextSV { get; set; }

        public string? PersonFI { get; set; }

        public string? PersonSV { get; set; }

        public string? Type { get; set; }

        public string? TypeTextFI { get; set; }

        public string? TypeTextSV { get; set; }
    }
}
