namespace Storage.Repositories.Models
{
    public class BreakNotice
    {
        public string MeetingID { get; set; }

        public Guid EventID { get; set; }

        public string Notice { get; set; }
    }
}
