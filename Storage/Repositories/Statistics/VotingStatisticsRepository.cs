using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Models.Statistics;
using Storage.Repositories.Providers;
using System.Data;

namespace Storage.Repositories.Statistics
{
    /// <summary>
    /// Provides data access methods for voting statistics aggregated by person.
    /// </summary>
    public interface IVotingStatisticsRepository
    {
        /// <summary>
        /// Retrieves voting statistics for a specific year.
        /// </summary>
        /// <param name="year">The year to retrieve statistics for.</param>
        /// <returns>A list of voting statistics with vote counts by type per person.</returns>
        Task<List<VotingStatistics>> GetStatistics(int year);
    }


    /// <summary>
    /// Implements voting statistics data access using Dapper for PostgreSQL queries.
    /// Aggregates vote counts by person and vote type (for, against, empty, absent).
    /// </summary>
    public class VotingStatisticsRepository : IVotingStatisticsRepository
    {
        private readonly ILogger<VotingStatisticsRepository> _logger;
        private readonly IDatabaseConnectionFactory _connectionFactory;

        /// <summary>
        /// Initializes a new instance of the VotingStatisticsRepository class.
        /// </summary>
        /// <param name="logger">Logger for diagnostic information.</param>
        /// <param name="connectionFactory">Factory for creating database connections.</param>
        public VotingStatisticsRepository(ILogger<VotingStatisticsRepository> logger, IDatabaseConnectionFactory connectionFactory)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Retrieves voting statistics for all persons in a year.
        /// Counts votes by type: 0=for, 1=against, 2=empty, 3=absent.
        /// </summary>
        /// <param name="year">The year to retrieve statistics for (4-digit year).</param>
        /// <returns>A list of voting statistics grouped by person with vote type counts.</returns>
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
