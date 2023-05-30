using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Models.Statistics;
using Storage.Repositories.Providers;
using System.Data;

namespace Storage.Repositories.Statistics
{
    public interface IVotingStatisticsRepository
    {
        Task<List<VotingStatistics>> GetStatistics(int year);
    }


    public class VotingStatisticsRepository : IVotingStatisticsRepository
    {
        private readonly ILogger<VotingStatisticsRepository> _logger;
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public VotingStatisticsRepository(ILogger<VotingStatisticsRepository> logger, IDatabaseConnectionFactory connectionFactory)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        public async Task<List<VotingStatistics>> GetStatistics(int year)
        {
            var meetingId = $"02900{year}%";
            string query = @"
                select
                    person,
                    additional_info_fi,
                    sum(case when vote_type = 0 then 1 else 0 end) as for,
                    sum(case when vote_type = 1 then 1 else 0 end) as against,
                    sum(case when vote_type = 2 then 1 else 0 end) as empty,
                    sum(case when vote_Type = 3 then 1 else 0 end) as absent,
                    count(vote_Type) as sum
                from
                    votes where meeting_id like @meetingId group by (person, additional_info_fi);";

            using var connection = await _connectionFactory.CreateOpenConnection();
            return (await connection.QueryAsync<VotingStatistics>(query, new { meetingId })).ToList();
        }
    }
}
