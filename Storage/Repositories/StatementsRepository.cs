using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;
using System.Data;

namespace Storage.Repositories
{
    public interface IStatementsRepository
    {
        Task InsertStartedStatement(StartedStatement startedStatement, IDbConnection connection, IDbTransaction transaction);

        Task UpsertStatements(List<Statement> statements, IDbConnection connection, IDbTransaction transaction);

        Task InsertStatementReservation(StatementReservation statementReservation, IDbConnection connection, IDbTransaction transaction);

        Task InsertReplyReservation(ReplyReservation replyReservation, IDbConnection connection, IDbTransaction transaction);

        Task<List<Statement>> GetStatements(string meetingId, string agendaPoint);

        Task<List<Statement>> GetSatementsByName(string name, int year, string lang);
    }

    public class StatementsRepository : IStatementsRepository
    {
        private readonly ILogger<StatementsRepository> _logger;
        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;

        public StatementsRepository(
            ILogger<StatementsRepository> logger,
            IDatabaseConnectionFactory databaseConnectionFactory)
        {
            _logger = logger;
            _databaseConnectionFactory = databaseConnectionFactory;
        }

        public async Task<List<Statement>> GetSatementsByName(string name, int year, string lang)
        {
            var sqlQuery = @"
                select distinct
                    statements.meeting_id,
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
                    statements
                join
                    meeting_events on statements.event_id = meeting_events.event_id
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
                    statements.meeting_id,
                    person,
                    started,
                    ended,
                    speech_type,
                    duration_seconds,
                    additional_info_fi,
                    additional_info_sv
                from
                    statements
                join
                    meeting_events on statements.event_id = meeting_events.event_id
                where
                    meeting_events.meeting_id = @meetingId and meeting_events.case_number = @agendaPoint
                order by statements.started asc
            ";

            using var connection = await _databaseConnectionFactory.CreateOpenConnection();

            return (await connection.QueryAsync<Statement>(sqlQuery, new { meetingId, agendaPoint })).ToList();
        }


        public Task InsertStartedStatement(StartedStatement startedStatements, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"insert into started_statements (meeting_id, event_id, timestamp, person, speaking_time, speech_timer, start_time, 
                direction, seat_id, speech_type, additional_info_fi, additional_info_sv) values (
                @meetingId, 
                @eventId,
                @timestamp,
                @person,
                @speakingTime,
                @speechTimer,
                @startTime,
                @direction,
                @seatId,
                @speechType, 
                @additionalInfoFi,
                @additionalInfoSv
            )";

            return connection.ExecuteAsync(sqlQuery, startedStatements, transaction);
        }

        public Task UpsertStatements(List<Statement> statements, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Executing UpsertStatements()");
            var sqlQuery = @"INSERT INTO statements (meeting_id, event_id, person, started, ended, speech_type, duration_seconds, 
                additional_info_fi, additional_info_sv) values(
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
                WHERE statements.meeting_id = @meetingID and statements.started = @started
            ;";

            return connection.ExecuteAsync(sqlQuery, statements.Select(item => new
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

        public Task InsertStatementReservation(StatementReservation statementReservation, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"insert into statement_reservations (meeting_id, event_id, timestamp, person, ordinal, seat_id, additional_info_fi, 
                additional_info_sv) values (
                @meetingId,
                @eventId,
                @timestamp,
                @person,
                @ordinal,
                @seatId,
                @additionalInfoFi,
                @additionalInfoSv
            )";

            return connection.ExecuteAsync(sqlQuery, statementReservation, transaction);
        }

        public Task InsertReplyReservation(ReplyReservation replyReservation, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"INSERT INTO reply_reservations (meeting_id, event_id, person, additional_info_fi, additional_info_sv) values(
                @meetingId, 
                @eventId,
                @person,
                @additionalInfoFi,
                @additionalInfoSv
            ) ";

            return connection.ExecuteAsync(sqlQuery, replyReservation, transaction);
        }
    }
}