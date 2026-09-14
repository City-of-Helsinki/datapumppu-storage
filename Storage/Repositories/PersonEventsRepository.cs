using System.Data;
using Dapper;
using Storage.Repositories.Models;

namespace Storage.Repositories
{
    /// <summary>
    /// Provides data access methods for person-related meeting events.
    /// </summary>
    public interface IPersonEventsRepository
    {
        /// <summary>
        /// Inserts a person event (arrival, departure, seat change) into the database.
        /// </summary>
        /// <param name="personEvent">The person event to insert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InsertPersonEvent(PersonEvent personEvent, IDbConnection connection, IDbTransaction transaction);
    }

    /// <summary>
    /// Implements person event data access operations using Dapper for PostgreSQL queries.
    /// Tracks person movements and status changes during meetings with bilingual additional info.
    /// </summary>
    public class PersonEventsRepository: IPersonEventsRepository
    {
        private readonly ILogger<PersonEventsRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the PersonEventsRepository class.
        /// </summary>
        /// <param name="logger">Logger for diagnostic information.</param>
        public PersonEventsRepository(ILogger<PersonEventsRepository> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Inserts a person event into the database, recording participant movements or status changes.
        /// </summary>
        /// <param name="personEvent">The person event entity with timestamp, person name, event type, and seat information.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task InsertPersonEvent(PersonEvent personEvent, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Executing InsertPersonEvent()");
            var sqlQuery = @"insert into person_events (meeting_id, event_id, timestamp, person, event_type, 
                seat_id, additional_info_fi, additional_info_sv) values(               
                @meetingId,
                @eventId,
                @timestamp,
                @person,
                @eventType,
                @seatId,
                @additionalInfoFi,
                @additionalInfoSv
            )";

            return connection.ExecuteAsync(sqlQuery, personEvent, transaction);
        }
    }
}
