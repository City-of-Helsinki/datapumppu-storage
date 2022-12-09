using Dapper;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Repositories
{
    public interface IRollCallRepository
    {
        Task UpsertRollCallStarted(RollCall rollCall, IDbConnection connection, IDbTransaction transaction);

        Task UpsertRollCallEnded(RollCall rollCall, IDbConnection connection, IDbTransaction transaction);
    }

    public class RollCallRepository: IRollCallRepository
    {
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
