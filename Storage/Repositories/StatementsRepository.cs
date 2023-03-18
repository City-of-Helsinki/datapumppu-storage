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

        Task<List<StatementReservation>> GetStatementReservations(string meetingId, string agendaPoint);

        Task<List<ReplyReservation>> GetReplyReservations(string meetingId, string agendaPoint);
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

        public async Task<List<StatementReservation>> GetStatementReservations(string meetingId, string agendaPoint)
        {
            using var connection = await _databaseConnectionFactory.CreateOpenConnection();

            var timestampQuery = @$"
                SELECT timestamp 
                FROM meeting_events 
                WHERE meeting_id = @meetingId 
                AND case_number = @agendaPoint
                AND event_type = '{(int)EventType.StatementReservationsCleared}'
                ORDER BY timestamp DESC
                LIMIT 1";
            var lastClearedTimestamp = await connection.QueryFirstOrDefaultAsync<DateTime>(timestampQuery, new { meetingId, agendaPoint });

            var sqlQuery = @$"
                SELECT DISTINCT
                    statement_reservations.meeting_id, 
                    case_number, 
                    statement_reservations.timestamp, 
                    person, 
                    additional_info_fi, 
                    additional_info_sv, 
                    ordinal, 
                    seat_id
                FROM 
                    statement_reservations
                JOIN 
                    meeting_events 
                    ON statement_reservations.event_id = meeting_events.event_id
                WHERE 
                    statement_reservations.meeting_id = @meetingId 
                    AND case_number = @agendaPoint 
                    AND statement_reservations.timestamp >= TO_TIMESTAMP('{lastClearedTimestamp.ToString("dd.MM.yyyy HH:mm:ss")}', 'DD.MM.YYYY HH24:MI:SS')";

            var reservations = (await connection.QueryAsync<StatementReservation>(sqlQuery, new { meetingId, agendaPoint })).ToList();

            var activeStatement = await GetActiveStatement(meetingId, agendaPoint);
            foreach (var reservation in reservations)
            {
                reservation.Active = activeStatement != null ? reservation.Person == activeStatement.Person : false;
            }

            return reservations;
        }

        public async Task<List<ReplyReservation>> GetReplyReservations(string meetingId, string agendaPoint)
        {
            using var connection = await _databaseConnectionFactory.CreateOpenConnection();

            var timestampQuery = @$"
                SELECT timestamp 
                FROM meeting_events 
                WHERE meeting_id = @meetingId 
                AND case_number = @agendaPoint
                AND event_type = '{(int)EventType.ReplyReservationsCleared}'
                ORDER BY timestamp DESC
                LIMIT 1";
            var lastClearedTimestamp = await connection.QueryFirstOrDefaultAsync<DateTime>(timestampQuery, new { meetingId, agendaPoint });

            var sqlQuery = @$"
                SELECT DISTINCT
                    reply_reservations.meeting_id, 
                    case_number, 
                    reply_reservations.timestamp, 
                    person, 
                    additional_info_fi, 
                    additional_info_sv, 
                    ordinal, 
                    seat_id
                FROM 
                    reply_reservations
                JOIN 
                    meeting_events 
                    ON reply_reservations.event_id = meeting_events.event_id
                WHERE 
                    reply_reservations.meeting_id = @meetingId 
                    AND case_number = @agendaPoint 
                    AND reply_reservations.timestamp >= TO_TIMESTAMP('{lastClearedTimestamp}', 'DD.MM.YYYY HH24:MI:SS')";

            var reservations = (await connection.QueryAsync<ReplyReservation>(sqlQuery, new { meetingId, agendaPoint })).ToList();

            var activeStatement = await GetActiveStatement(meetingId, agendaPoint);
            foreach (var reservation in reservations)
            {
                reservation.Active = activeStatement != null ? reservation.Person == activeStatement.Person : false;
            }

            return reservations;
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
            var sqlQuery = @"INSERT INTO reply_reservations (meeting_id, event_id, person, additional_info_fi, additional_info_sv, ordinal, seat_id, timestamp) values(
                @meetingId, 
                @eventId,
                @person,
                @additionalInfoFi,
                @additionalInfoSv,
                @ordinal,
                @seatId,
                @timestamp
            ) ";

            return connection.ExecuteAsync(sqlQuery, replyReservation, transaction);
        }

        private async Task<StartedStatement?> GetActiveStatement(string meetingId, string agendaPoint)
        {
            using var connection = await _databaseConnectionFactory.CreateOpenConnection();
            _logger.LogInformation("Executing GetActiveStatement()");
            var sqlQuery1 = @$"
                SELECT timestamp 
                FROM meeting_events
                WHERE meeting_id = @meetingId
                AND case_number = @agendaPoint
                AND event_type = '{(int)EventType.StatementEnded}'
            ";
            var lastStatementEnded = await connection.QueryFirstOrDefaultAsync<DateTime>(sqlQuery1, new { meetingId, agendaPoint });

            var sqlQuery2 = $@"
                SELECT started_statements.meeting_id, started_statements.event_id, started_statements.timestamp, person, speaking_time, speech_timer, start_time, direction, seat_id, speech_type, additional_info_fi, additional_info_sv FROM started_statements
                JOIN meeting_events
                ON started_statements.event_id = meeting_events.event_id
                WHERE start_time > TO_TIMESTAMP('{lastStatementEnded}', 'DD.MM.YYYY HH24:MI:SS')
                AND started_statements.meeting_id = @meetingId
                AND meeting_events.case_number = @agendaPoint
                ORDER BY timestamp DESC
                LIMIT 1
            ";
            var result = await connection.QueryAsync<StartedStatement>(sqlQuery2, new { meetingId, agendaPoint });
            
            if (result.Any())
            {
                return result.First();
            }

            return null;
        }

    }
}