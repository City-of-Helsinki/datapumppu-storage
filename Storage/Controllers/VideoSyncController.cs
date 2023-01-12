using Microsoft.AspNetCore.Mvc;
using Storage.Actions;
using Storage.Controllers.MeetingInfo.DTOs;

namespace Storage.Controllers
{
    [ApiController]
    [Route("api/videosync/")]
    public class VideoSyncController : ControllerBase
    {
        private readonly ILogger<VideoSyncController> _logger;
        private readonly IUpsertVideoSyncItemAction _upsertVideoSyncItemAction;

        public VideoSyncController(ILogger<VideoSyncController> logger, IUpsertVideoSyncItemAction upsertVideoSyncItemAction)
        {
            _logger = logger;
            _upsertVideoSyncItemAction = upsertVideoSyncItemAction;
        }

        [HttpPost("position")]
        public async Task<IActionResult> PostVideoSyncItem([FromBody] VideoSyncDTO videoSyncDTO)
        {
            try
            {
                _logger.LogInformation("HTTP POST: videoSyncItem received");
                await _upsertVideoSyncItemAction.Execute(videoSyncDTO);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostVideoSync() failed");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

    }
}
