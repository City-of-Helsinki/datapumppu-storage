using Dapper;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Repositories
{
    public interface ISpeakingTurnsRepository
    {
        Task InsertSpeakingTurnReservation(SpeakingTurnReservation speakingTurnReservation, IDbConnection connection, IDbTransaction transaction);

        Task InsertStartedSpeakingTurn(StartedSpeakingTurn startedSpeakingTurn, IDbConnection connection, IDbTransaction transaction);

        Task UpsertSpeakingTurns(List<SpeakingTurn> speakingTurns, IDbConnection connection, IDbTransaction transaction);
    }

    public class SpeakingTurnsRepository : ISpeakingTurnsRepository
    {
        private readonly ILogger<SpeakingTurnsRepository> _logger;

        public SpeakingTurnsRepository(ILogger<SpeakingTurnsRepository> logger)
        {
            _logger = logger;
        }

        public Task InsertStartedSpeakingTurn(StartedSpeakingTurn startedSpeakingTurn, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"insert into started_speaking_turns (meeting_id, event_id, timestamp, person_fi, person_sv, speaking_time, speech_timer, start_time, direction, seat_id, speech_type) values (
                @meetingId, 
                @eventId,
                @timestamp,
                @personFi,
                @personSv,
                @speakingTime,
                @speechTimer,
                @startTime,
                @direction,
                @seatId,
                @speechType
            )";

            return connection.ExecuteAsync(sqlQuery, startedSpeakingTurn, transaction);
        }

        public Task InsertSpeakingTurnReservation(SpeakingTurnReservation speakingTurnReservation, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"insert into speaking_turn_reservations (meeting_id, event_id, timestamp, person_fi, person_sv, ordinal, seat_id) values (
                @meetingId,
                @eventId,
                @timestamp,
                @personFi,
                @personSv,
                @ordinal,
                @seatId
            )";

            return connection.ExecuteAsync(sqlQuery, speakingTurnReservation, transaction);
        }

        public Task UpsertSpeakingTurns(List<SpeakingTurn> speakingTurns, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Executing UpsertSpeakingTurns()");
            var sqlQuery = @"INSERT INTO speaking_turns (meeting_id, event_id, person_fi, person_sv, started, ended, speech_type, duration_seconds) values(
                @meetingId,
                @eventId,
                @personFi,
                @personSv,
                @started,
                @ended,
                @speechType,
                @durationSeconds
            ) ";
            sqlQuery += @"ON CONFLICT (meeting_id, started) DO UPDATE SET 
                event_id = @eventId,
                person_fi = @personFi,
                person_sv = @personSv,
                started = @started,
                ended = @ended,
                speech_type = @speechType,
                duration_seconds = @durationSeconds
                WHERE speaking_turns.meeting_id = @meetingID and speaking_turns.started = @started
            ;";

            return connection.ExecuteAsync(sqlQuery, speakingTurns.Select(item => new
            {
                meetingId = item.MeetingID,
                eventId = item.EventID,
                personFi = item.PersonFI,
                personSv = item.PersonSV,
                started = item.StartTime,
                ended = item.EndTime,
                speechType = item.SpeechType,
                durationSeconds = item.Duration
            }), transaction);
        }
    }
}