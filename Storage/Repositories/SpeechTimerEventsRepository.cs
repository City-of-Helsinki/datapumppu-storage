using Dapper;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Repositories
{
    /// <summary>
    /// Provides data access methods for speech timer event management.
    /// </summary>
    public interface ISpeechTimerEventsRepository
    {
        /// <summary>
        /// Inserts a speech timer event recording speaking time and duration.
        /// </summary>
        /// <param name="speechTimerEvent">The speech timer event to insert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InsertSpeechTimerEvent(SpeechTimerEvent speechTimerEvent, IDbConnection connection, IDbTransaction transaction);
    }

    /// <summary>
    /// Implements speech timer event data access operations using Dapper for PostgreSQL queries.
    /// Records speech timing events with duration, direction, and participant information.
    /// </summary>
    public class SpeechTimerEventsRepository : ISpeechTimerEventsRepository
    {
        /// <summary>
        /// Inserts a speech timer event into the database, tracking speech duration and timing information.
        /// </summary>
        /// <param name="speechTimerEvent">The speech timer event entity with duration, timer state, and participant details.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task InsertSpeechTimerEvent(SpeechTimerEvent speechTimerEvent, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"INSERT INTO speech_timer_events (meeting_id, event_id, seat_id, person, duration_seconds, speech_timer, 
                direction, additional_info_fi, additional_info_sv) values(
                @meetingId, 
                @eventId,
                @seatId, 
                @person,
                @durationSeconds,
                @speechTimer,
                @direction,
                @additionalInfoFi,
                @additionalInfoSv
            ); ";

            return connection.ExecuteAsync(sqlQuery, speechTimerEvent, transaction);
        }
    }
}