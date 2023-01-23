using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;

namespace Storage.Repositories
{
    public interface IVideoSyncRepository
    {
        Task<int> UpsertVideoSyncItem(VideoSync videoSyncItem);

        Task<VideoSync?> GetVideoPosition(string meetingId, DateTime timestamp);

        Task<List<VideoSync>> GetVideoPositions(string meetingId);
    }

    public class VideoSyncRepository : IVideoSyncRepository
    {
        private readonly ILogger<VideoSyncRepository> _logger;
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public VideoSyncRepository(IDatabaseConnectionFactory connectionFactory, ILogger<VideoSyncRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

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
