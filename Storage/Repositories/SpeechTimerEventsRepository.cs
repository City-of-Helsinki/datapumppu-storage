using Dapper;
using Microsoft.Extensions.Logging;
using Storage.Repositories.Models;
using System.Data;
using System.Security.Cryptography.Xml;

namespace Storage.Repositories
{
    public interface ISpeechTimerEventsRepository
    {
        Task InsertSpeechTimerEvent(SpeechTimerEvent speechTimerEvent, IDbConnection connection, IDbTransaction transaction);
    }

    public class SpeechTimerEventsRepository : ISpeechTimerEventsRepository
    {
        public Task InsertSpeechTimerEvent(SpeechTimerEvent speechTimerEvent, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"INSERT INTO speech_timer_events (meeting_id, event_id, seat_id, person_fi, person_sv, duration_seconds, speech_timer, direction) values(
                @meetingId, 
                @eventId,
                @seatId, 
                @personFi,
                @personSv,
                @durationSeconds,
                @speechTimer,
                @direction
            ); ";

            return connection.ExecuteAsync(sqlQuery, speechTimerEvent, transaction);
        }
    }
}