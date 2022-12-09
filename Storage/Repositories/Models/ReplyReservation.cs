namespace Storage.Repositories.Models
{
    public class ReplyReservation
    {
        public string MeetingID { get; set; }

        public Guid EventID { get; set; }

        public string PersonFI { get; set; }

        public string PersonSV { get; set; }
    }
}
