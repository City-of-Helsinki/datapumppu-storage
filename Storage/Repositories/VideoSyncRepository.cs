using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;

namespace Storage.Repositories
{
    /// <summary>
    /// Provides data access methods for video synchronization management.
    /// </summary>
    public interface IVideoSyncRepository
    {
        /// <summary>
        /// Inserts or updates a video synchronization item linking meeting timestamps to video positions.
        /// </summary>
        /// <param name="videoSyncItem">The video sync item to upsert.</param>
        /// <returns>The number of rows affected.</returns>
        Task<int> UpsertVideoSyncItem(VideoSync videoSyncItem);

        /// <summary>
        /// Retrieves the video position for a given meeting timestamp.
        /// Returns the most recent sync point before the specified timestamp.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="timestamp">The meeting timestamp to find the video position for.</param>
        /// <returns>The video sync information if found, otherwise null.</returns>
        Task<VideoSync?> GetVideoPosition(string meetingId, DateTime timestamp);

        /// <summary>
        /// Retrieves all video synchronization points for a meeting.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <returns>A list of all video sync points for the meeting.</returns>
        Task<List<VideoSync>> GetVideoPositions(string meetingId);
    }

    /// <summary>
    /// Implements video synchronization data access operations using Dapper for PostgreSQL queries.
    /// Maps meeting event timestamps to video playback positions for synchronized viewing.
    /// </summary>
    public class VideoSyncRepository : IVideoSyncRepository
    {
        private readonly ILogger<VideoSyncRepository> _logger;
        private readonly IDatabaseConnectionFactory _connectionFactory;

        /// <summary>
        /// Initializes a new instance of the VideoSyncRepository class.
        /// </summary>
        /// <param name="connectionFactory">Factory for creating database connections.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        public VideoSyncRepository(IDatabaseConnectionFactory connectionFactory, ILogger<VideoSyncRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the video position for a meeting at the most recent sync point before the specified timestamp.
        /// Used to determine where in the video playback a meeting event occurred.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="timestamp">The meeting timestamp to find the video position for.</param>
        /// <returns>The video sync record with position information, or null if no sync point exists before the timestamp.</returns>
        public async Task<VideoSync?> GetVideoPosition(string meetingId, DateTime timestamp)
        {
            _logger.LogInformation("Executing GetVideoPosition()");

            string sqlQuery = @"
                select
                    meeting_id,
                    timestamp,
                    video_position
                from
                    video_synchronizations
                where
                    meeting_id = @meetingId
                    and
                    timestamp < @timestamp
                order by
                    timestamp desc
                limit 1";

            using var connection = await _connectionFactory.CreateOpenConnection();
            return (await connection.QueryAsync<VideoSync>(sqlQuery, new { meetingId, timestamp })).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves all video synchronization points for a meeting.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <returns>A list of all video sync points with timestamps and video positions.</returns>
        public async Task<List<VideoSync>> GetVideoPositions(string meetingId)
        {
            _logger.LogInformation("Executing GetVideoPosition()");

            string sqlQuery = @"
                select
                    meeting_id,
                    timestamp,
                    video_position
                from
                    video_synchronizations
                where
                    meeting_id = @meetingId";

            using var connection = await _connectionFactory.CreateOpenConnection();
            return (await connection.QueryAsync<VideoSync>(sqlQuery, new { meetingId })).ToList();
        }

        /// <summary>
        /// Inserts or updates a video synchronization item.
        /// Updates existing entries based on meeting_id and timestamp.
        /// </summary>
        /// <param name="videoSyncItem">The video sync item with meeting ID, timestamp, and video position.</param>
        /// <returns>The number of rows affected (typically 1).</returns>
        public async Task<int> UpsertVideoSyncItem(VideoSync videoSyncItem)
        {
            _logger.LogInformation("Executing UpsertVideoSyncData()");

            var sqlQuery = @"INSERT INTO video_synchronizations (meeting_id, timestamp, video_position) values(
                @meetingId, 
                @timestamp,
                @videoPosition
            ) ";
            sqlQuery += @"ON CONFLICT (meeting_id, timestamp) DO UPDATE SET 
                video_position = @videoPosition
                WHERE video_synchronizations.meeting_id = @meetingId and video_synchronizations.timestamp = @timestamp
            ;";

            using var connection = await _connectionFactory.CreateOpenConnection();
            return await connection.ExecuteAsync(sqlQuery, videoSyncItem);
        }

    }
}
