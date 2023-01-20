namespace Storage.Repositories.Models
{
    public class PauseInfo
    {
        public string MeetingID { get; set; }

        public Guid EventID { get; set; }

        public string Info { get; set; }
    }
}
