﻿using Storage.Controllers.MeetingInfo.DTOs;

namespace Storage.Providers.DTOs
{
    public class WebApiMeetingDTO
    {
        public DateTime MeetingDate { get; set; }

        public string MeetingID { get; set; }

        public string Name { get; set; }

        public int MeetingSequenceNumber { get; set; }

        public string Location { get; set; }

        public string? MeetingTitleFI { get; set; }

        public string? MeetingTitleSV { get; set; }

        public DateTime? MeetingStarted { get; set; }

        public Guid? MeetingStartedEventID { get; set; }

        public DateTime? MeetingEnded { get; set; }

        public Guid? MeetingEndedEventID { get; set; }

        public List<WebApiAgendaItemDTO>? Agendas { get; set; }

        public List<WebApiDecisionDTO>? Decisions { get; set; }
    }
}
