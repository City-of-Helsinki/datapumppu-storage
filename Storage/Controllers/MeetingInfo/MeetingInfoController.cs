using Microsoft.AspNetCore.Mvc;
using Storage.Actions;
using Storage.Providers;
using Storage.Controllers.MeetingInfo.DTOs;

namespace Storage.Controllers.MeetingInfo
{
    /// <summary>
    /// Provides API endpoints for managing and retrieving detailed meeting information including agendas and decisions.
    /// </summary>
    [ApiController]
    [Route("api/meetinginfo")]
    public class MeetingInfoController : ControllerBase
    {
        private readonly ILogger<MeetingInfoController> _logger;
        private readonly IUpsertMeetingAction _upsertMeetingAction;
        private readonly IUpsertAgendaPointAction _upsertAgendaPointAction;
        private readonly IMeetingProvider _meetingProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingInfoController"/> class.
        /// </summary>
        /// <param name="logger">Logger for recording controller operations.</param>
        /// <param name="upsertMeetingAction">Action for creating or updating meeting data.</param>
        /// <param name="upsertAgendaPointAction">Action for creating or updating agenda point data.</param>
        /// <param name="meetingProvider">Provider for meeting data operations.</param>
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

        /// <summary>
        /// Creates or updates meeting information including agendas and decisions.
        /// </summary>
        /// <param name="meetingDTO">The meeting data to be saved.</param>
        /// <returns>Returns 200 OK if the meeting was saved successfully.</returns>
        [HttpPost("meeting")]
        public async Task<IActionResult> UpsertMeeting([FromBody] MeetingDTO meetingDTO)
        {
            _logger.LogInformation("HTTP POST: meeting received");
            await _upsertMeetingAction.Execute(meetingDTO);
            return Ok();
        }

        /// <summary>
        /// Creates or updates a specific agenda point with editor information.
        /// </summary>
        /// <param name="agendaPointDTO">The agenda point data including meeting ID, agenda point number, editor details, and HTML content.</param>
        /// <returns>Returns 200 OK if successful, or 412 Precondition Failed if the operation could not be completed.</returns>
        [HttpPost("agendaPoint")]
        public async Task<IActionResult> UpsertAgendaPoint([FromBody] AgendaPointEditDTO agendaPointDTO)
        {
            _logger.LogInformation("HTTP POST: UpsertAgendaPoint");
            var success = await _upsertAgendaPointAction.Execute(agendaPointDTO);

            return success ? Ok() : StatusCode(StatusCodes.Status412PreconditionFailed);
        }

        /// <summary>
        /// Retrieves detailed meeting information by meeting ID.
        /// </summary>
        /// <param name="id">The unique identifier of the meeting.</param>
        /// <param name="language">The language code (e.g., 'fi' for Finnish, 'sv' for Swedish) for localized content.</param>
        /// <returns>Returns 200 OK with the meeting details including agendas and decisions.</returns>
        [HttpGet("meeting/{id}")]
        public async Task<IActionResult> GetMeetingById(string id, string language)
        {
            _logger.LogInformation("GetMeetingById: {0} {1}", id, language);
            var meeting = await _meetingProvider.FetchById(id, language);

            return Ok(meeting);
        }

        /// <summary>
        /// Retrieves sub-items for a specific agenda point within a meeting.
        /// </summary>
        /// <param name="id">The unique identifier of the meeting.</param>
        /// <param name="agendaPoint">The agenda point number.</param>
        /// <returns>Returns 200 OK with the list of agenda sub-items.</returns>
        [HttpGet("meeting/{id}/{agendaPoint}")]
        public async Task<IActionResult> GetMeetingAgendaSubItems(string id, int agendaPoint)
        {
            _logger.LogInformation("GetMeetingAgendaSubItems {0} {1}", id, agendaPoint);

            var items = await _meetingProvider.FetchAgendaSubItemsById(id, agendaPoint);

            return Ok(items);
        }

        /// <summary>
        /// Retrieves meeting information by year and sequence number.
        /// </summary>
        /// <param name="year">The year of the meeting.</param>
        /// <param name="sequenceNumber">The sequence number of the meeting within the year.</param>
        /// <param name="language">The language code (e.g., 'fi' for Finnish, 'sv' for Swedish) for localized content.</param>
        /// <returns>Returns 200 OK with the meeting details.</returns>
        [HttpGet("meeting/{year}/{sequenceNumber}/{language}")]
        public async Task<IActionResult> GetMeeting(string year, string sequenceNumber, string language)
        {
            _logger.LogInformation("GetMeeting {0} {1} {2}", year, sequenceNumber, language);
            var meeting = await _meetingProvider.FetchMeeting(year, sequenceNumber, language);
            return Ok(meeting);
        }

        /// <summary>
        /// Retrieves the meeting ID for a meeting identified by year and sequence number.
        /// </summary>
        /// <param name="year">The year of the meeting.</param>
        /// <param name="sequenceNumber">The sequence number of the meeting within the year.</param>
        /// <returns>Returns 200 OK with the meeting ID.</returns>
        [HttpGet("meetingId/{year}/{sequenceNumber}")]
        public async Task<IActionResult> GetMeetingId(string year, string sequenceNumber)
        {
            _logger.LogInformation("GetMeetingId {0} {1}", year, sequenceNumber);
            var meeting = await _meetingProvider.FetchMeetingId(year, sequenceNumber);
            return Ok(meeting);
        }


        /// <summary>
        /// Retrieves information about the next upcoming scheduled meeting.
        /// </summary>
        /// <param name="language">The language code (e.g., 'fi' for Finnish, 'sv' for Swedish) for localized content.</param>
        /// <returns>Returns 200 OK with the upcoming meeting details, or null if no upcoming meeting is scheduled.</returns>
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingMeeting(string language)
        {
            var meeting = await _meetingProvider.FetchNextUpcomingMeeting(language);

            return Ok(meeting);
        }

    }
}