using System.Data;
using Dapper;
using Storage.Repositories.Models;

namespace Storage.Repositories
{
    public interface IEventsRepository
    {
        Task InsertEvent(Event meetingEvent, IDbConnection connection, IDbTransaction transaction);
    }

    public class EventsRepository: IEventsRepository
    {
        private readonly ILogger<EventsRepository> _logger;

        public EventsRepository(ILogger<EventsRepository> logger)
        {
            _logger = logger;
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
