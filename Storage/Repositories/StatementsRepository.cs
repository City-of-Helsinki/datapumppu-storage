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

        Task<List<Statement>> GetSatementsByName(string name, int year);
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

        public async Task<List<Statement>> GetSatementsByName(string name, int year)
        {
            var sqlQuery = @"
                select
                    meeting_id,
                    person,
                    started,
                    ended,
                    speech_type,
                    duration_seconds,
                    additional_info_fi,
                    additional_info_sv
                from
                    speaking_turns
                where
                    lower(person) like (lower(@name))
                    and
                    extract(year from started) = @year
            ";

            using var connection = await _databaseConnectionFactory.CreateOpenConnection();

            return (await connection.QueryAsync<Statement>(sqlQuery, new { name, year })).ToList();
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
            ";

            using var connection = await _databaseConnectionFactory.CreateOpenConnection();

            return (await connection.QueryAsync<Statement>(sqlQuery, new { meetingId, agendaPoint })).ToList();
        }


        public Task InsertStartedStatement(StartedStatement startedStatements, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"insert into started_statements (meeting_id, event_id, timestamp, person_fi, person_sv, speaking_time, 
                speech_timer, start_time, direction, seat_id, speech_type) values (
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
            var sqlQuery = @"insert into statement_reservations (meeting_id, event_id, timestamp, person_fi, person_sv, ordinal, seat_id) values (
                @meetingId,
                @eventId,
                @timestamp,
                @personFi,
                @personSv,
                @ordinal,
                @seatId
            )";

            return connection.ExecuteAsync(sqlQuery, statementReservation, transaction);
        }

        public Task InsertReplyReservation(ReplyReservation replyReservation, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"INSERT INTO reply_reservations (meeting_id, event_id, person_fi, person_sv) values(
                @meetingId, 
                @eventId,
                @personFi, 
                @personSv
            ) ";

            return connection.ExecuteAsync(sqlQuery, replyReservation, transaction);
        }
    }
}