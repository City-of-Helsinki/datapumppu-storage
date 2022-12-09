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
        private readonly IMeetingProvider _meetingProvider;

        public MeetingInfoController(ILogger<MeetingInfoController> logger,
            IUpsertMeetingAction upsertMeetingAction, IMeetingProvider meetingProvider)
        {
            _logger = logger;
            _upsertMeetingAction = upsertMeetingAction;
            _meetingProvider = meetingProvider;
        }

        [HttpPost("meeting")]
        public async Task<IActionResult> UpsertMeeting([FromBody] MeetingDTO meetingDTO)
        {
            _logger.LogInformation("HTTP POST: meeting received");
            await _upsertMeetingAction.Execute(meetingDTO);
            return Ok();
        }

        [HttpGet("meeting/{id}")]
        public async Task<IActionResult> GetMeeting(string id)
        {
            var meeting = await _meetingProvider.FetchById(id);

            return Ok(meeting);
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingMeeting()
        {
            var meeting = await _meetingProvider.FetchNextUpcomingMeeting();

            return Ok(meeting);
        }

    }
}