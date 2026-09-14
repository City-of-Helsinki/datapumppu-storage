using System.Data;
using System.Transactions;
using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;

namespace Storage.Repositories
{
    /// <summary>
    /// Provides data access methods for meeting event management.
    /// </summary>
    public interface IEventsRepository
    {
        /// <summary>
        /// Inserts a meeting event into the database.
        /// </summary>
        /// <param name="meetingEvent">The event to insert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InsertEvent(Event meetingEvent, IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Checks if a specific agenda point has already been handled in a meeting.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="caseNumber">The agenda point case number.</param>
        /// <returns>True if the agenda point has events recorded, otherwise false.</returns>
        Task<bool> IsAgendaPointHandled(string meetingId, string caseNumber);
    }

    /// <summary>
    /// Implements meeting event data access operations using Dapper for PostgreSQL queries.
    /// Stores all meeting events with timestamps, sequence numbers, and agenda point associations.
    /// </summary>
    public class EventsRepository: IEventsRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly ILogger<EventsRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the EventsRepository class.
        /// </summary>
        /// <param name="logger">Logger for diagnostic information.</param>
        /// <param name="databaseConnectionFactory">Factory for creating database connections.</param>
        public EventsRepository(ILogger<EventsRepository> logger,
            IDatabaseConnectionFactory databaseConnectionFactory)
        {
            _logger = logger;
            _connectionFactory = databaseConnectionFactory;
        }

        /// <summary>
        /// Checks if any events exist for the specified agenda point in a meeting.
        /// Used to determine if an agenda item has been processed during the meeting.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="caseNumber">The case number representing the agenda point.</param>
        /// <returns>True if at least one event exists for the agenda point, otherwise false.</returns>
        public async Task<bool> IsAgendaPointHandled(string meetingId, string caseNumber)
        {
            var sqlQuery = "select * from meeting_events where meeting_id = @meetingId and case_number = @caseNumber limit 1";

            using var connection = await _connectionFactory.CreateOpenConnection();
            var result = await connection.QueryAsync(sqlQuery, new { meetingId, caseNumber });
            return result.Any();
        }

        /// <summary>
        /// Inserts a new meeting event into the database.
        /// </summary>
        /// <param name="meetingEvent">The event entity containing event details, timestamp, and sequence information.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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
