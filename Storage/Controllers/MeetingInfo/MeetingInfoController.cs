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
            var success = await _upsertAgendaPointAction.Execute(agendaPointDTO);

            return success ? Ok() : StatusCode(StatusCodes.Status412PreconditionFailed);
        }

        [HttpGet("meeting/{id}")]
        public async Task<IActionResult> GetMeetingById(string id, string language)
        {
            _logger.LogInformation("GetMeetingById: {0} {1}", id, language);
            var meeting = await _meetingProvider.FetchById(id, language);

            return Ok(meeting);
        }

        [HttpGet("meeting/{id}/{agendaPoint}")]
        public async Task<IActionResult> GetMeetingAgendaSubItems(string id, int agendaPoint)
        {
            _logger.LogInformation("GetMeetingAgendaSubItems {0} {1}", id, agendaPoint);

            var items = await _meetingProvider.FetchAgendaSubItemsById(id, agendaPoint);

            return Ok(items);
        }

        [HttpGet("meeting/{year}/{sequenceNumber}/{language}")]
        public async Task<IActionResult> GetMeeting(string year, string sequenceNumber, string language)
        {
            _logger.LogInformation("GetMeeting {0} {1} {2}", year, sequenceNumber, language);
            var meeting = await _meetingProvider.FetchMeeting(year, sequenceNumber, language);
            return Ok(meeting);
        }

        [HttpGet("meetingId/{year}/{sequenceNumber}")]
        public async Task<IActionResult> GetMeetingId(string year, string sequenceNumber)
        {
            _logger.LogInformation("GetMeetingId {0} {1}", year, sequenceNumber);
            var meeting = await _meetingProvider.FetchMeetingId(year, sequenceNumber);
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