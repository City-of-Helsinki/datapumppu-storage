using Microsoft.AspNetCore.Mvc;
using Storage.Actions;
using Storage.Providers;
using Storage.Controllers.MeetingInfo.DTOs;

namespace Storage.Controllers.MeetingInfo
{
    [ApiController]
    [Route("api/meetinginfo")]
    public class MeetingInfoController : ControllerBase
    {
        private readonly ILogger<MeetingInfoController> _logger;
        private readonly IUpsertMeetingAction _upsertMeetingAction;
        private readonly IUpsertAgendaPointAction _upsertAgendaPointAction;
        private readonly IMeetingProvider _meetingProvider;

        public MeetingInfoController(
            ILogger<MeetingInfoController> logger,
            IUpsertMeetingAction upsertMeetingAction,
            IUpsertAgendaPointAction upsertAgendaPointAction,
            IMeetingProvider meetingProvider)
        {
            _logger = logger;
            _upsertMeetingAction = upsertMeetingAction;
            _meetingProvider = meetingProvider;
            _upsertAgendaPointAction = upsertAgendaPointAction;
        }

        [HttpPost("meeting")]
        public async Task<IActionResult> UpsertMeeting([FromBody] MeetingDTO meetingDTO)
        {
            _logger.LogInformation("HTTP POST: meeting received");
            await _upsertMeetingAction.Execute(meetingDTO);
            return Ok();
        }

        [HttpPost("agendaPoint")]
        public async Task<IActionResult> UpsertAgendaPoint([FromBody] AgendaPointEditDTO agendaPointDTO)
        {
            _logger.LogInformation("HTTP POST: UpsertAgendaPoint");
            await _upsertAgendaPointAction.Execute(agendaPointDTO);
            return Ok();
        }

        [HttpGet("meeting/{id}")]
        public async Task<IActionResult> GetMeetingById(string id, string language)
        {
            var meeting = await _meetingProvider.FetchById(id, language);

            return Ok(meeting);
        }

        [HttpGet("meeting/{year}/{sequenceNumber}/{language}")]
        public async Task<IActionResult> GetMeeting(string year, string sequenceNumber, string language)
        {
            var meeting = await _meetingProvider.FetchMeeting(year, sequenceNumber, language);
            return Ok(meeting);
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingMeeting(string language)
        {
            var meeting = await _meetingProvider.FetchNextUpcomingMeeting(language);

            return Ok(meeting);
        }

    }
}