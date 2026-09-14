using Dapper;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Repositories
{
    /// <summary>
    /// Provides data access methods for roll call management.
    /// </summary>
    public interface IRollCallRepository
    {
        /// <summary>
        /// Inserts or updates the start time of a roll call for a meeting.
        /// </summary>
        /// <param name="rollCall">The roll call start information to upsert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpsertRollCallStarted(RollCall rollCall, IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Inserts or updates the end time and attendance counts for a roll call.
        /// </summary>
        /// <param name="rollCall">The roll call end information including present and absent counts.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpsertRollCallEnded(RollCall rollCall, IDbConnection connection, IDbTransaction transaction);
    }

    /// <summary>
    /// Implements roll call data access operations using Dapper for PostgreSQL queries.
    /// Manages meeting attendance tracking with start/end times and attendance counts.
    /// </summary>
    public class RollCallRepository: IRollCallRepository
    {
        /// <summary>
        /// Inserts or updates the roll call start information using an upsert operation.
        /// Updates existing roll call if one exists for the meeting ID.
        /// </summary>
        /// <param name="rollCall">The roll call entity with start timestamp and event ID.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task UpsertRollCallStarted(RollCall rollCall, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"INSERT INTO roll_calls (meeting_id, roll_call_started, roll_call_started_eventid) values(
                @meetingId, 
                @rollCallStarted, 
                @rollCallStartedEventId
            ) ";
            sqlQuery += @"ON CONFLICT (meeting_id) DO UPDATE SET 
                roll_call_started = @rollCallStarted,
                roll_call_started_eventid = @rollCallStartedEventId
                WHERE roll_calls.meeting_id = @meetingId
            ;";

            return connection.ExecuteAsync(sqlQuery, rollCall, transaction);
        }

        /// <summary>
        /// Inserts or updates the roll call end information including attendance counts.
        /// Updates existing roll call if one exists for the meeting ID.
        /// </summary>
        /// <param name="rollCall">The roll call entity with end timestamp, event ID, and present/absent counts.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task UpsertRollCallEnded(RollCall rollCall, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"INSERT INTO roll_calls (meeting_id, roll_call_ended, roll_call_ended_eventid, present, absent) values(
                @meetingId, 
                @rollCallEnded, 
                @rollCallEndedEventId,
                @present,
                @absent
            ) ";
            sqlQuery += @"ON CONFLICT (meeting_id) DO UPDATE SET 
                roll_call_ended = @rollCallEnded,
                roll_call_ended_eventid = @rollCallEndedEventId,
                present = @present,
                absent = @absent
                WHERE roll_calls.meeting_id = @meetingId
            ;";

            return connection.ExecuteAsync(sqlQuery, rollCall, transaction);
        }
    }
}
