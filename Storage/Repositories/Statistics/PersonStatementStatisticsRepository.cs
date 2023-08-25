using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Models.Statistics;
using Storage.Repositories.Providers;
using System.Data;

namespace Storage.Repositories.Statistics
{
    public interface IPersonStatementStatisticsRepository
    {
        Task<List<PersonStatementStatistics>> GetStatistics(int year);
    }


    public class PersonStatementStatisticsRepository : IPersonStatementStatisticsRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public PersonStatementStatisticsRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

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
