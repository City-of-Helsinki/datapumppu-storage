using Microsoft.AspNetCore.Mvc;
using Storage.Actions;
using Storage.Controllers.MeetingInfo.DTOs;

namespace Storage.Controllers
{
    /// <summary>
    /// Provides API endpoints for synchronizing video playback positions with meeting events.
    /// </summary>
    [ApiController]
    [Route("api/videosync/")]
    public class VideoSyncController : ControllerBase
    {
        private readonly ILogger<VideoSyncController> _logger;
        private readonly IUpsertVideoSyncItemAction _upsertVideoSyncItemAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoSyncController"/> class.
        /// </summary>
        /// <param name="logger">Logger for recording controller operations.</param>
        /// <param name="upsertVideoSyncItemAction">Action for upserting video synchronization data.</param>
        public VideoSyncController(ILogger<VideoSyncController> logger, IUpsertVideoSyncItemAction upsertVideoSyncItemAction)
        {
            _logger = logger;
            _upsertVideoSyncItemAction = upsertVideoSyncItemAction;
        }

        /// <summary>
        /// Records or updates a video synchronization point linking a meeting timestamp to a video position.
        /// </summary>
        /// <param name="videoSyncDTO">The video synchronization data containing meeting ID, timestamp, and video position.</param>
        /// <returns>Returns 200 OK if the synchronization data was saved successfully, or 500 Internal Server Error if the operation fails.</returns>
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
