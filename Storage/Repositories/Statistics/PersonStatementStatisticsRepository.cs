using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Models.Statistics;
using Storage.Repositories.Providers;
using System.Data;

namespace Storage.Repositories.Statistics
{
    /// <summary>
    /// Provides data access methods for person-level statement statistics.
    /// </summary>
    public interface IPersonStatementStatisticsRepository
    {
        /// <summary>
        /// Retrieves statement statistics for all persons for a specific year.
        /// </summary>
        /// <param name="year">The year to retrieve statistics for.</param>
        /// <returns>A list of person statement statistics with durations and agenda titles.</returns>
        Task<List<PersonStatementStatistics>> GetStatistics(int year);
    }


    /// <summary>
    /// Implements person statement statistics data access using Dapper for PostgreSQL queries.
    /// Aggregates statement data per person showing speaking time and agenda context.
    /// </summary>
    public class PersonStatementStatisticsRepository : IPersonStatementStatisticsRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        /// <summary>
        /// Initializes a new instance of the PersonStatementStatisticsRepository class.
        /// </summary>
        /// <param name="connectionFactory">Factory for creating database connections.</param>
        public PersonStatementStatisticsRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Retrieves statement statistics for all persons in a year, including duration and agenda titles.
        /// Joins statements with meeting events and agenda items (Finnish titles).
        /// </summary>
        /// <param name="year">The year to retrieve statistics for (4-digit year).</param>
        /// <returns>A list of person statement statistics ordered by person, meeting, and title.</returns>
        public async Task<List<PersonStatementStatistics>> GetStatistics(int year)
        {
            var meetingId = $"02900{year}%";
            string query = @"
                select
                    statements.person,
                    statements.meeting_id,
                    agenda_items.title,
                    statements.started,
                    statements.ended,
                    statements.duration_seconds
                from
                    statements
                left join meeting_events
                    on statements.event_id = meeting_events.event_id
                left join agenda_items
                    on meeting_events.case_number = agenda_items.agenda_point::varchar(255)
                        and meeting_events.meeting_id = agenda_items.meeting_id
                        and agenda_items.language = 'fi'
                where
                    statements.meeting_id like @meetingId order by person, meeting_id, title";

            using var connection = await _connectionFactory.CreateOpenConnection();
            return (await connection.QueryAsync<PersonStatementStatistics>(query, new { meetingId })).ToList();
        }
    }
}
