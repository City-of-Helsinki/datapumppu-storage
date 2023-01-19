namespace Storage.Events.DTOs
{
    public class VoteDTO
    {
        public string Person { get; set; }

        public VoteType VoteType { get; set; }

        public string AdditionalInfoFI { get; set; }

        public string AdditionalInfoSV { get; set; }
    }
}
