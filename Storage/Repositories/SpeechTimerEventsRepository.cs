using Dapper;
using Storage.Repositories.Models;
using System.Data;

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