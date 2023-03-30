using System.Data;
using System.Transactions;
using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;

namespace Storage.Repositories
{
    public interface IEventsRepository
    {
        Task InsertEvent(Event meetingEvent, IDbConnection connection, IDbTransaction transaction);

        Task<bool> IsAgendaPointHandled(string meetingId, string caseNumber);
    }

    public class EventsRepository: IEventsRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly ILogger<EventsRepository> _logger;

        public EventsRepository(ILogger<EventsRepository> logger,
            IDatabaseConnectionFactory databaseConnectionFactory)
        {
            _logger = logger;
            _connectionFactory = databaseConnectionFactory;
        }

        public async Task<bool> IsAgendaPointHandled(string meetingId, string caseNumber)
        {
            var sqlQuery = "select * from meeting_events where meeting_id = @meetingId and case_number = @caseNumber limit 1";

            using var connection = await _connectionFactory.CreateOpenConnection();
            var result = await connection.QueryAsync(sqlQuery, new { meetingId, caseNumber });
            return result.Any();
        }

        public Task InsertEvent(Event meetingEvent, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Executing InsertEvent()");
            var sqlQuery = @"insert into meeting_events (meeting_id, event_id, event_type, timestamp, sequence_number, case_number, item_number) values(               
                @meetingId,
                @eventId,
                @eventType,
                @timestamp,
                @sequenceNumber,
                @caseNumber,
                @itemNumber
            )";

            return connection.ExecuteAsync(sqlQuery, meetingEvent, transaction);
        }
    }
}
