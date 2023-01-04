namespace Storage.Controllers.MeetingInfo.DTOs
{
    public class WebApiVotesDTO
    {
        public string? ForTitleFI { get; set; }
        
        public string? ForTitleSV { get; set; }

        public string? AgainstTitleFI { get; set; }

        public string? AgainstTitleSV { get; set; }

        public int ForCount { get; set; }

        public int AgainstCount { get; set; }

        public int EmptyCount { get; set; }

        public int AbsentCount { get; set; }

        public WebApiVoteDTO[] Votes { get; set; }
    }
}