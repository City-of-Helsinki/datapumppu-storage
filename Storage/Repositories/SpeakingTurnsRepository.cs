using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;
using System.Data;

namespace Storage.Repositories
{
    public interface ISpeakingTurnsRepository
    {
        Task InsertSpeakingTurnReservation(SpeakingTurnReservation speakingTurnReservation, IDbConnection connection, IDbTransaction transaction);

        Task InsertStartedSpeakingTurn(StartedStatement startedSpeakingTurn, IDbConnection connection, IDbTransaction transaction);

        Task UpsertSpeakingTurns(List<Statement> speakingTurns, IDbConnection connection, IDbTransaction transaction);

        Task<List<Statement>> GetStatements(string meetingId, string agendaPoint);

        Task<List<Statement>> GetSatementsByName(string name, int year, string lang);
    }

    public class SpeakingTurnsRepository : ISpeakingTurnsRepository
    {
        private readonly ILogger<SpeakingTurnsRepository> _logger;
        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;

        public SpeakingTurnsRepository(
            ILogger<SpeakingTurnsRepository> logger,
            IDatabaseConnectionFactory databaseConnectionFactory)
        {
            _logger = logger;
            _databaseConnectionFactory = databaseConnectionFactory;
        }

        public async Task<List<Statement>> GetSatementsByName(string name, int year, string lang)
        {
            var sqlQuery = @"
                select distinct
                    speaking_turns.meeting_id,
                    person,
                    started,
                    ended,
                    speech_type,
                    duration_seconds,
                    additional_info_fi,
                    additional_info_sv,
                    agenda_items.title as title,
                    agenda_items.agenda_point as case_number
                from
                    speaking_turns
                join
                    meeting_events on speaking_turns.event_id = meeting_events.event_id
                join
                    agenda_items on 
                        meeting_events.meeting_id = agenda_items.meeting_id and
                        agenda_items.agenda_point = meeting_events.case_number::int8
                where
                    person = @name
                    and
                    extract(year from started) = @year
                    and
                    agenda_items.language = @lang
            ";

            using var connection = await _databaseConnectionFactory.CreateOpenConnection();

            return (await connection.QueryAsync<Statement>(sqlQuery, new { name, year, lang })).ToList();
        }

        public async Task<List<Statement>> GetStatements(string meetingId, string agendaPoint)
        {
            var sqlQuery = @"
                select
                    speaking_turns.meeting_id,
                    person,
                    started,
                    ended,
                    speech_type,
                    duration_seconds,
                    additional_info_fi,
                    additional_info_sv
                from
                    speaking_turns
                join
                    meeting_events on speaking_turns.event_id = meeting_events.event_id
                where
                    meeting_events.meeting_id = @meetingId and meeting_events.case_number = @agendaPoint
            ";

            using var connection = await _databaseConnectionFactory.CreateOpenConnection();

            return (await connection.QueryAsync<Statement>(sqlQuery, new { meetingId, agendaPoint })).ToList();
        }


        public Task InsertStartedSpeakingTurn(StartedStatement startedSpeakingTurn, IDbConnection connection, IDbTransaction transaction)
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

        public Task UpsertSpeakingTurns(List<Statement> speakingTurns, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Executing UpsertSpeakingTurns()");
            var sqlQuery = @"INSERT INTO speaking_turns (meeting_id, event_id, person, started, ended, speech_type, duration_seconds, additional_info_fi, additional_info_sv) values(
                @meetingId,
                @eventId,
                @person,
                @started,
                @ended,
                @speechType,
                @durationSeconds,
                @additionalInfoFi,
                @additionalInfoSv
            ) ";
            sqlQuery += @"ON CONFLICT (meeting_id, started) DO UPDATE SET 
                event_id = @eventId,
                person = @person,
                started = @started,
                ended = @ended,
                speech_type = @speechType,
                duration_seconds = @durationSeconds,
                additional_info_fi = @additionalInfoFi,
                additional_info_sv = @additionalInfoSv
                WHERE speaking_turns.meeting_id = @meetingID and speaking_turns.started = @started
            ;";

            return connection.ExecuteAsync(sqlQuery, speakingTurns.Select(item => new
            {
                meetingId = item.MeetingID,
                eventId = item.EventID,
                person = item.Person,
                started = item.Started,
                ended = item.Ended,
                speechType = item.SpeechType,
                durationSeconds = item.DurationSeconds,
                additionalInfoFi = item.AdditionalInfoFI,
                additionalInfoSv = item.AdditionalInfoSV
            }), transaction);
        }
    }
}