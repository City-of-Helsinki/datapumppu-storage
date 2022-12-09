namespace Storage.Repositories.Models
{
    public class RollCall
    {
        public string MeetingID { get; set; }

        public Guid? RollCallStartedEventID { get; set; }

        public DateTime? RollCallStarted { get; set; }

        public Guid? RollCallEndedEventID { get; set; }

        public DateTime? RollCallEnded { get; set; }

        public int? Present { get; set; }

        public int? Absent { get; set; }
    }
}
