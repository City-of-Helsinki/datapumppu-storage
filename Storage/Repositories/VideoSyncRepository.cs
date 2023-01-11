using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;

namespace Storage.Repositories
{
    public interface IVideoSyncRepository
    {
        Task<int> UpsertVideoSyncItem(VideoSync videoSyncItem);
    }

    public class VideoSyncRepository: IVideoSyncRepository
    {
        private readonly ILogger<VideoSyncRepository> _logger;
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public VideoSyncRepository(IDatabaseConnectionFactory connectionFactory, ILogger<VideoSyncRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<int> UpsertVideoSyncItem(VideoSync videoSyncItem)
        {
            _logger.LogInformation("Executing UpsertVideoSyncData()");

            var sqlQuery = @"INSERT INTO video_synchronizations (meeting_id, timestamp, video_position) values(
                @meetingId, 
                @timestamp,
                @videoPosition
            ) ";
            sqlQuery += @"ON CONFLICT (meeting_id, video_position) DO UPDATE SET 
                timestamp = @timestamp
                WHERE video_synchronizations.meeting_id = @meetingId and video_synchronizations.video_position = @videoPosition
            ;";

            using var connection = await _connectionFactory.CreateOpenConnection();
            return await connection.ExecuteAsync(sqlQuery, videoSyncItem);
        }

    }
}
