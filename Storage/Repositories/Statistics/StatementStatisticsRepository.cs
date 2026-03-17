using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Models.Statistics;
using Storage.Repositories.Providers;
using System.Data;

namespace Storage.Repositories.Statistics
{
    /// <summary>
    /// Provides data access methods for statement statistics aggregated by meeting and case.
    /// </summary>
    public interface IStatementStatisticsRepository
    {
        /// <summary>
        /// Retrieves statement statistics for a specific year.
        /// </summary>
        /// <param name="year">The year to retrieve statistics for.</param>
        /// <returns>A list of statement statistics with counts and durations per case.</returns>
        Task<List<StatementStatistics>> GetStatistics(int year);
    }


    /// <summary>
    /// Implements statement statistics data access using Dapper for PostgreSQL queries.
    /// Aggregates statement counts and durations by meeting and case, identifies motions.
    /// </summary>
    public class StatementStatisticsRepository : IStatementStatisticsRepository
    {
        private readonly ILogger<StatementStatisticsRepository> _logger;
        private readonly IDatabaseConnectionFactory _connectionFactory;

        /// <summary>
        /// Initializes a new instance of the StatementStatisticsRepository class.
        /// </summary>
        /// <param name="logger">Logger for diagnostic information.</param>
        /// <param name="connectionFactory">Factory for creating database connections.</param>
        public StatementStatisticsRepository(ILogger<StatementStatisticsRepository> logger, IDatabaseConnectionFactory connectionFactory)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Retrieves statement statistics aggregated by meeting and case for a year.
        /// Includes count, total duration, and identifies motions (title contains 'aloite').
        /// </summary>
        /// <param name="year">The year to retrieve statistics for (4-digit year).</param>
        /// <returns>A list of statement statistics grouped by meeting and case.</returns>
        public async Task<List<StatementStatistics>> GetStatistics(int year)
        {
            var meetingId = $"02900{year}%";
            string query = @"
                select
                    statements.meeting_id as meeting_id,
                    meeting_events.case_number as case_number,
                    agenda_items.title as title,
                    count(statements.*) as count,
                    sum(statements.duration_seconds) as total_duration,
                    case when agenda_items.title ilike '%aloite %' then true else false end as is_motion
                from
                    statements
                left join meeting_events
                    on statements.event_id = meeting_events.event_id
                left join agenda_items
                    on statements.meeting_id = agenda_items.meeting_id and case_number = agenda_items.agenda_point::varchar(10) and agenda_items.language = 'fi'
                where
                    statements.meeting_id like @meetingId
                group by
                    (
                        statements.meeting_id,
                        case_number,
                        title
                    )";

            using var connection = await _connectionFactory.CreateOpenConnection();
            return (await connection.QueryAsync<StatementStatistics>(query, new { meetingId })).ToList();
        }
    }
}
