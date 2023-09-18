using Storage.Repositories.Models;
using Storage.Repositories.Providers;
using Dapper;
using System.Data;

namespace Storage.Repositories
{
    public interface IMeetingsRepository
    {
        Task UpsertMeeting(Meeting meeting, IDbConnection connection, IDbTransaction transaction);

        Task UpsertMeetingStartTime(Meeting meeting, IDbConnection connection, IDbTransaction transaction);

        Task UpdateMeetingEndTime(Meeting meeting, IDbConnection connection, IDbTransaction transaction);

        Task<Meeting?> FetchMeetingById(string id);

        Task<Meeting?> FetchNextUpcomingMeeting();

        Task<Meeting?> FetchMeetingByYearAndSeuquenceNumber(string Year, string sequenceNumber);
    }

    public class MeetingsRepository : IMeetingsRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly ILogger<MeetingsRepository> _logger;

        public MeetingsRepository(IDatabaseConnectionFactory connectionFactory, ILogger<MeetingsRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

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
