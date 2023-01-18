namespace Storage.Repositories.Models
{
    public class Proposition
    {
        public string MeetingID { get; set; }

        public Guid EventID { get; set; }

        public string? TextFI { get; set; }

        public string? TextSV { get; set; }

        public string? Person { get; set; }

        public string? Type { get; set; }

        public string? TypeTextFI { get; set; }

        public string? TypeTextSV { get; set; }

        public string? AdditionalInfoFI { get; set; }

        public string? AdditionalInfoSV { get; set; }
    }
}
