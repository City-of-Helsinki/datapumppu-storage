using Storage.Repositories.Models;
using Storage.Repositories.Providers;
using Dapper;
using System.Data;

namespace Storage.Repositories
{
    /// <summary>
    /// Provides data access methods for meeting management and retrieval.
    /// </summary>
    public interface IMeetingsRepository
    {
        /// <summary>
        /// Inserts or updates a meeting's basic information.
        /// </summary>
        /// <param name="meeting">The meeting entity to upsert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpsertMeeting(Meeting meeting, IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Inserts or updates a meeting's start time information.
        /// </summary>
        /// <param name="meeting">The meeting entity with start time data.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpsertMeetingStartTime(Meeting meeting, IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Updates the end time of a meeting.
        /// </summary>
        /// <param name="meeting">The meeting entity with end time data.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateMeetingEndTime(Meeting meeting, IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Retrieves a meeting by its identifier.
        /// </summary>
        /// <param name="id">The meeting identifier.</param>
        /// <returns>The meeting if found, otherwise null.</returns>
        Task<Meeting?> FetchMeetingById(string id);

        /// <summary>
        /// Retrieves the next upcoming meeting scheduled after the current time.
        /// </summary>
        /// <returns>The next upcoming meeting if found, otherwise null.</returns>
        Task<Meeting?> FetchNextUpcomingMeeting();

        /// <summary>
        /// Retrieves a meeting by year and sequence number.
        /// </summary>
        /// <param name="Year">The year of the meeting.</param>
        /// <param name="sequenceNumber">The meeting sequence number within the year.</param>
        /// <returns>The meeting if found, otherwise null.</returns>
        Task<Meeting?> FetchMeetingByYearAndSeuquenceNumber(string Year, string sequenceNumber);
    }

    /// <summary>
    /// Implements meeting data access operations using Dapper for PostgreSQL queries.
    /// Manages meeting lifecycle including creation, start/end times, and metadata.
    /// </summary>
    public class MeetingsRepository : IMeetingsRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly ILogger<MeetingsRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the MeetingsRepository class.
        /// </summary>
        /// <param name="connectionFactory">Factory for creating database connections.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        public MeetingsRepository(IDatabaseConnectionFactory connectionFactory, ILogger<MeetingsRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a meeting by its unique identifier.
        /// </summary>
        /// <param name="id">The meeting identifier.</param>
        /// <returns>The meeting entity if found, otherwise null.</returns>
        public async Task<Meeting?> FetchMeetingById(string id)
        {
            using var connection = await _connectionFactory.CreateOpenConnection();
            var sqlQuery = @"
                SELECT
                    meeting_id,
                    name,
                    location,
                    meeting_date,
                    meeting_started,
                    meeting_sequence_number
                FROM
                    meetings
                WHERE
                    meeting_id = @id
            ";
            var result = (await connection.QueryAsync<Meeting>(sqlQuery, new { @id })).ToList();

            return result.SingleOrDefault();
        }

        /// <summary>
        /// Retrieves the next meeting scheduled after the current date/time, ordered by meeting date ascending.
        /// </summary>
        /// <returns>The next upcoming meeting if found, otherwise null.</returns>
        public async Task<Meeting?> FetchNextUpcomingMeeting()
        {
            using var connection = await _connectionFactory.CreateOpenConnection();
            var sqlQuery = @"
                SELECT meeting_id, name, location, meeting_date 
                FROM meetings  
                WHERE meeting_date > NOW() 
                ORDER BY meeting_date ASC;
            ";          

            var result = (await connection.QueryAsync<Meeting>(sqlQuery)).ToList();

            return result.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves a meeting by year and sequence number within that year.
        /// </summary>
        /// <param name="year">The year of the meeting (e.g., "2024").</param>
        /// <param name="sequenceNumber">The sequence number of the meeting in that year.</param>
        /// <returns>The meeting if found, otherwise null.</returns>
        public async Task<Meeting?> FetchMeetingByYearAndSeuquenceNumber(string year, string sequenceNumber)
        {
            using var connection = await _connectionFactory.CreateOpenConnection();
            var firstDayOfYear = $"{year}-01-01";
            var lastDayOfYear = $"{year}-12-31T23:59:59";
            var sqlQuery = @$"
                SELECT * FROM meetings
                WHERE meeting_date >= '{firstDayOfYear}'::date AND meeting_date <= '{lastDayOfYear}'::date AND meeting_sequence_number = {sequenceNumber};
            "; 
            var result = (await connection.QueryAsync<Meeting>(sqlQuery)).ToList();

            return result.FirstOrDefault();
        }

        /// <summary>
        /// Inserts a new meeting or updates an existing one based on whether the meeting already exists.
        /// </summary>
        /// <param name="meeting">The meeting entity to upsert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpsertMeeting(Meeting meeting, IDbConnection connection, IDbTransaction transaction)
        {

            if (await MeetingExists(meeting.MeetingID, connection, transaction))
            {
                await UpdateMeeting(meeting, connection, transaction);
            }
            else
            {
                await InsertMeeting(meeting, connection, transaction);
            }
        }

        /// <summary>
        /// Inserts a new meeting with start time or updates the start time of an existing one.
        /// </summary>
        /// <param name="meeting">The meeting entity with start time information.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpsertMeetingStartTime(Meeting meeting, IDbConnection connection, IDbTransaction transaction)
        {
            if (await MeetingExists(meeting.MeetingID, connection, transaction))
            {
                await UpdateMeetingStartTime(meeting, connection, transaction);
            }
            else
            {
                await InsertMeetingWithStartTime(meeting, connection, transaction);
            }
        }

        /// <summary>
        /// Updates the end time and title information for a meeting.
        /// </summary>
        /// <param name="meeting">The meeting entity with end time information.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task UpdateMeetingEndTime(Meeting meeting, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Executing UpdateMeetingEndTime()");
            var sqlQuery = @"update meetings set
                meeting_title_fi = @meetingTitleFi,
                meeting_title_sv = @meetingTitleSv,
                meeting_ended = @meetingEnded,
                meeting_ended_eventid = @meetingEndedEventId
                where meeting_id = @meetingId
            ";

            return connection.ExecuteAsync(sqlQuery, meeting, transaction);
        }

        private Task UpdateMeetingStartTime(Meeting meeting, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Executing UpdateMeetingStartTime()");
            var sqlQuery = @"update meetings set
                meeting_title_fi = @meetingTitleFi,
                meeting_title_sv = @meetingTitleSv,
                meeting_started = @meetingStarted,
                meeting_started_eventid = @meetingStartedEventId
                where meeting_id = @meetingId
            ";

            return connection.ExecuteAsync(sqlQuery, meeting, transaction);
        }

        private Task InsertMeetingWithStartTime(Meeting meeting, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Executing InsertMeetingWithStartTime()");
            var sqlQuery = @"insert into meetings (meeting_id, meeting_title_fi, meeting_title_sv, meeting_started, meeting_started_eventid) values (
                @meetingId,
                @meetingTitleFi,
                @meetingTitleSv,
                @meetingStarted,
                @meetingStartedEventId
            )";

            return connection.ExecuteAsync(sqlQuery, meeting, transaction);
        }

        private Task UpdateMeeting(Meeting meeting, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Executing UpdateMeeting()");
            var sqlQuery = @"update meetings set
                name = @name,
                meeting_date = @meetingDate,
                meeting_sequence_number = @meetingSequenceNumber,
                location = @location
                where meeting_id = @meetingId
            ";

            return connection.ExecuteAsync(sqlQuery, meeting, transaction);
        }

        private Task InsertMeeting(Meeting meeting, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Executing InsertMeeting()");
            var sqlQuery = @"insert into meetings (meeting_id, name, meeting_date, meeting_sequence_number, location, 
                meeting_title_fi, meeting_title_sv, meeting_started, meeting_started_eventid, meeting_ended, 
                meeting_ended_eventid) values (
                @meetingId,
                @name,
                @meetingDate,
                @meetingSequenceNumber,
                @location,
                @meetingTitleFi,
                @meetingTitleSv,
                @meetingStarted,
                @meetingStartedEventId,
                @meetingEnded,
                @meetingEndedEventId
            )";

            return connection.ExecuteAsync(sqlQuery, meeting, transaction);
        }

        private async Task<bool> MeetingExists(string meetingId, IDbConnection connection, IDbTransaction? transaction = null)
        {
            var sqlQuery = "select count(meeting_id) from meetings where meeting_id = @MeetingId";
            var count = (await connection.QueryAsync<int>(sqlQuery, new { MeetingId = meetingId }, transaction)).Single();
            return count == 1;
        }

    }
}
