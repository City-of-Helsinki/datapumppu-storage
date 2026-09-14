using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;
using System.Data;

namespace Storage.Repositories
{
    /// <summary>
    /// Provides data access methods for statement, reservation, and active speaker management.
    /// </summary>
    public interface IStatementsRepository
    {
        /// <summary>
        /// Inserts a started statement record into the database.
        /// </summary>
        /// <param name="startedStatement">The started statement to insert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InsertStartedStatement(StartedStatement startedStatement, IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Inserts or updates completed statements with start and end times.
        /// </summary>
        /// <param name="statements">The list of statements to upsert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpsertStatements(List<Statement> statements, IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Inserts a statement reservation into the database.
        /// </summary>
        /// <param name="statementReservation">The statement reservation to insert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InsertStatementReservation(StatementReservation statementReservation, IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Inserts a reply reservation into the database.
        /// </summary>
        /// <param name="replyReservation">The reply reservation to insert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InsertReplyReservation(ReplyReservation replyReservation, IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Retrieves all statements for a specific agenda point in a meeting.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="agendaPoint">The agenda point number.</param>
        /// <returns>A list of statements ordered by start time.</returns>
        Task<List<Statement>> GetStatements(string meetingId, string agendaPoint);

        /// <summary>
        /// Retrieves statements made by a specific person in a given year and language.
        /// </summary>
        /// <param name="name">The person's name to search for.</param>
        /// <param name="year">The year to filter statements.</param>
        /// <param name="lang">The language code for agenda item titles.</param>
        /// <returns>A list of statements with agenda titles.</returns>
        Task<List<Statement>> GetSatementsByName(string name, int year, string lang);

        /// <summary>
        /// Retrieves statements filtered by person names and/or date range.
        /// </summary>
        /// <param name="names">List of person names to filter by (supports partial matching).</param>
        /// <param name="startDate">Optional start date filter.</param>
        /// <param name="endDate">Optional end date filter.</param>
        /// <param name="lang">The language code for agenda item titles.</param>
        /// <returns>A list of statements matching the filters.</returns>
        Task<List<Statement>> GetStatementsByPersonOrDate(List<string> names, DateTime? startDate, DateTime? endDate, string lang);

        /// <summary>
        /// Retrieves all statement reservations for a specific agenda point up to and including the given point.
        /// Filters out reservations cleared before the last StatementReservationsCleared event.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="agendaPoint">The agenda point number.</param>
        /// <returns>A list of statement reservations.</returns>
        Task<List<StatementReservation>> GetStatementReservations(string meetingId, string agendaPoint);

        /// <summary>
        /// Retrieves all reply reservations for a specific agenda point up to and including the given point.
        /// Filters out reservations cleared before the last ReplyReservationsCleared event.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="agendaPoint">The agenda point number.</param>
        /// <returns>A list of reply reservations.</returns>
        Task<List<ReplyReservation>> GetReplyReservations(string meetingId, string agendaPoint);

        /// <summary>
        /// Retrieves the currently active speaker for an agenda point.
        /// Determines the active speaker from the most recent started statement after the last ended statement.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="agendaPoint">The agenda point number.</param>
        /// <returns>A reply reservation representing the active speaker, or null if no active speaker.</returns>
        Task<ReplyReservation?> GetActiveSpeaker(string meetingId, string agendaPoint);
    }

    /// <summary>
    /// Implements statement and reservation data access operations using Dapper for PostgreSQL queries.
    /// Manages meeting statements, reservations, and active speaker tracking with complex temporal queries.
    /// </summary>
    public class StatementsRepository : IStatementsRepository
    {
        private readonly ILogger<StatementsRepository> _logger;
        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;

        /// <summary>
        /// Initializes a new instance of the StatementsRepository class.
        /// </summary>
        /// <param name="logger">Logger for diagnostic information.</param>
        /// <param name="databaseConnectionFactory">Factory for creating database connections.</param>
        public StatementsRepository(
            ILogger<StatementsRepository> logger,
            IDatabaseConnectionFactory databaseConnectionFactory)
        {
            _logger = logger;
            _databaseConnectionFactory = databaseConnectionFactory;
        }

        /// <summary>
        /// Retrieves statements filtered by person names and optional date range.
        /// Performs case-insensitive partial matching on person names, supporting multiple word names.
        /// </summary>
        /// <param name="names">List of person names to search for (partial matches supported).</param>
        /// <param name="startDate">Optional start date to filter statements (inclusive).</param>
        /// <param name="endDate">Optional end date to filter statements (inclusive).</param>
        /// <param name="lang">Language code for agenda item titles ('fi' or 'sv').</param>
        /// <returns>A list of statements with associated agenda information.</returns>
        public async Task<List<Statement>> GetStatementsByPersonOrDate(List<string> names, DateTime? startDate, DateTime? endDate, string lang)
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
                where 1=1
            ";

            var parameters = new DynamicParameters();
            parameters.Add("Language", lang);

            if (names != null && names.Any())
            {
                var nameConditions = new List<string>();

                for (int i = 0; i < names.Count; i++)
                {
                    var name = names[i];
                    var words = name.Split(' ').Select(word => word.Trim()).ToList();
                    var wordConditions = new List<string>();
                    for (int j = 0; j < words.Count; j++)
                    {
                        var paramName = $"name_{i}_{j}";
                        parameters.Add(paramName, $"%{words[j]}%");
                        wordConditions.Add($"person ILIKE @{paramName}");
                    }

                    nameConditions.Add("(" + string.Join(" AND ", wordConditions) + ")");
                }

                sqlQuery += " AND (" + string.Join(" OR ", nameConditions) + ")";
            }

            if (startDate.HasValue)
            {
                sqlQuery += " AND started >= @StartDate";
                parameters.Add("StartDate", startDate.Value);
            }
            if (endDate.HasValue)
            {
                sqlQuery += " AND ended <= @EndDate";
                parameters.Add("EndDate", endDate.Value);
            }

            sqlQuery += " AND agenda_items.language = @Language";

            using var connection = await _databaseConnectionFactory.CreateOpenConnection();

            return (await connection.QueryAsync<Statement>(sqlQuery, parameters)).ToList();
        }


        /// <summary>
        /// Retrieves all statements made by a specific person in a given year.
        /// Joins with agenda items to include agenda titles in the specified language.
        /// </summary>
        /// <param name="name">The exact name of the person to search for.</param>
        /// <param name="year">The year to filter statements.</param>
        /// <param name="lang">Language code for agenda item titles.</param>
        /// <returns>A list of statements with agenda point and title information.</returns>
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
                    agenda_items
                        ON meeting_events.meeting_id = agenda_items.meeting_id
                        AND meeting_events.case_number ~ '^\d+(\.\d+)?$'
                        AND FLOOR(CAST(meeting_events.case_number AS numeric))::BIGINT = agenda_items.agenda_point
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

        /// <summary>
        /// Retrieves all statements for a specific agenda point, ordered by start time.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="agendaPoint">The agenda point number as a string.</param>
        /// <returns>A list of statements with person, timing, and additional bilingual information.</returns>
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
                    additional_info_sv,
                    meeting_events.item_number
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

        /// <summary>
        /// Retrieves statement reservations for an agenda point, excluding those cleared by StatementReservationsCleared events.
        /// Returns all reservations for agenda points up to and including the specified point.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="agendaPoint">The agenda point number as a string.</param>
        /// <returns>A list of active statement reservations with person and seat information.</returns>
        public async Task<List<StatementReservation>> GetStatementReservations(string meetingId, string agendaPoint)
        {
            using var connection = await _databaseConnectionFactory.CreateOpenConnection();

            var timestampQuery = @"
                SELECT timestamp 
                FROM meeting_events 
                WHERE meeting_id = @meetingId 
                AND event_type = @eventType
                ORDER BY timestamp DESC
                LIMIT 1";
            var lastClearedTimestamp = await connection.QueryFirstOrDefaultAsync<DateTime>(timestampQuery, new { meetingId, eventType = ((int)EventType.StatementReservationsCleared).ToString() });

            int integerAgendaPoint = Int32.Parse(agendaPoint);
            var sqlQuery = @"
                SELECT DISTINCT
                    statement_reservations.meeting_id, 
                    case_number, 
                    statement_reservations.timestamp, 
                    person, 
                    additional_info_fi, 
                    additional_info_sv, 
                    ordinal, 
                    seat_id,
                    meeting_events.item_number
                FROM 
                    statement_reservations
                JOIN 
                    meeting_events 
                    ON statement_reservations.event_id = meeting_events.event_id
                WHERE 
                    statement_reservations.meeting_id = @meetingId 
                    AND nullif(case_number, '')::int <= @integerAgendaPoint
                    AND statement_reservations.timestamp >= @lastClearedTimestamp";

            return (await connection.QueryAsync<StatementReservation>(sqlQuery, new { meetingId, integerAgendaPoint, lastClearedTimestamp })).ToList();
        }

        /// <summary>
        /// Retrieves the currently active speaker by finding the most recent started statement after the last ended statement.
        /// Converts the active statement to a ReplyReservation with the Active flag set.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="agendaPoint">The agenda point number as a string.</param>
        /// <returns>A reply reservation representing the active speaker, or null if no active speaker.</returns>
        public async Task<ReplyReservation?> GetActiveSpeaker(string meetingId, string agendaPoint)
        {
            var activeStatement = await GetActiveStatement(meetingId, agendaPoint);
            if (activeStatement == null)
            {
                return null;
            }

            return new ReplyReservation
            {
                Active = true,
                AdditionalInfoFI = activeStatement.AdditionalInfoFI,
                AdditionalInfoSV = activeStatement.AdditionalInfoSV,
                CaseNumber = Int32.Parse(agendaPoint),
                MeetingID = meetingId,
                Ordinal = 0,
                Person = activeStatement.Person,
                SeatID = activeStatement.SeatID,
                ItemNumber = activeStatement.ItemNumber
            };
        }

        /// <summary>
        /// Retrieves reply reservations for an agenda point, excluding those cleared by ReplyReservationsCleared events.
        /// Returns all reservations for agenda points up to and including the specified point.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="agendaPoint">The agenda point number as a string.</param>
        /// <returns>A list of active reply reservations with person and seat information.</returns>
        public async Task<List<ReplyReservation>> GetReplyReservations(string meetingId, string agendaPoint)
        {
            using var connection = await _databaseConnectionFactory.CreateOpenConnection();

            var timestampQuery = @"
                SELECT timestamp 
                FROM meeting_events 
                WHERE meeting_id = @meetingId 
                AND event_type = @eventType
                ORDER BY timestamp DESC
                LIMIT 1";
            var lastClearedTimestamp = await connection.QueryFirstOrDefaultAsync<DateTime>(timestampQuery, new { meetingId, eventType = ((int)EventType.ReplyReservationsCleared).ToString() });

            int integerAgendaPoint = Int32.Parse(agendaPoint);
            var sqlQuery = @"
                SELECT DISTINCT
                    reply_reservations.meeting_id, 
                    case_number, 
                    reply_reservations.timestamp, 
                    person, 
                    additional_info_fi, 
                    additional_info_sv, 
                    ordinal, 
                    seat_id,
                    meeting_events.item_number
                FROM 
                    reply_reservations
                JOIN 
                    meeting_events 
                    ON reply_reservations.event_id = meeting_events.event_id
                WHERE 
                    reply_reservations.meeting_id = @meetingId 
                    AND nullif(case_number, '')::int <= @integerAgendaPoint
                    AND reply_reservations.timestamp >= @lastClearedTimestamp";

            return (await connection.QueryAsync<ReplyReservation>(sqlQuery, new { meetingId, integerAgendaPoint, lastClearedTimestamp })).ToList();
        }

        /// <summary>
        /// Inserts a started statement record indicating when a person begins speaking.
        /// </summary>
        /// <param name="startedStatements">The started statement entity with timing and speaker information.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Inserts or updates completed statements with start and end times.
        /// Updates are based on meeting_id and started timestamp.
        /// </summary>
        /// <param name="statements">The list of completed statement entities to upsert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Inserts a statement reservation indicating someone has reserved to speak.
        /// </summary>
        /// <param name="statementReservation">The statement reservation entity with person and ordinal position.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Inserts a reply reservation indicating someone has reserved to reply to a statement.
        /// </summary>
        /// <param name="replyReservation">The reply reservation entity with person and ordinal position.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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
            var sqlQuery1 = @"
                SELECT timestamp 
                FROM meeting_events
                WHERE meeting_id = @meetingId
                AND case_number = @agendaPoint
                AND event_type = @eventType
                ORDER BY timestamp desc
                LIMIT 1
            ";
            var lastStatementEnded = await connection.QueryFirstOrDefaultAsync<DateTime>(sqlQuery1, new { meetingId, agendaPoint, eventType = ((int)EventType.StatementEnded).ToString() });

            var sqlQuery2 = @"
                SELECT
                    started_statements.meeting_id,
                    started_statements.event_id,
                    started_statements.timestamp,
                    person,
                    speaking_time,
                    speech_timer,
                    start_time,
                    direction, seat_id, speech_type, additional_info_fi, additional_info_sv,
                    meeting_events.item_number
                FROM
                    started_statements
                JOIN
                    meeting_events
                ON started_statements.event_id = meeting_events.event_id
                WHERE start_time > @lastStatementEnded
                AND started_statements.meeting_id = @meetingId
                AND meeting_events.case_number = @agendaPoint
                ORDER BY timestamp DESC
                LIMIT 1
            ";
            var result = await connection.QueryAsync<StartedStatement>(sqlQuery2, new { meetingId, agendaPoint, lastStatementEnded });
            
            if (result.Any())
            {
                return result.First();
            }

            return null;
        }

    }
}