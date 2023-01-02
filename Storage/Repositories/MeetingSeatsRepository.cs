using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;
using System.Data;
using System.Net.WebSockets;
using System.Transactions;

namespace Storage.Repositories
{
    public interface IMeetingSeatsRepository
    {
        Task InsertMeetingSeatUpdate(MeetingSeatUpdate meetingSeatUpdate, List<MeetingSeat> meetingSeats, IDbConnection connection, IDbTransaction transaction);

        Task<int> GetUpdateId(string meetingId, string caseNumber);

        Task<List<MeetingSeat>> GetSeats(int updateId);
    }

    public class MeetingSeatsRepository : IMeetingSeatsRepository
    {
        private readonly ILogger<MeetingSeatsRepository> _logger;
        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;

        public MeetingSeatsRepository(ILogger<MeetingSeatsRepository> logger,
            IDatabaseConnectionFactory databaseConnectionFactory)
        {
            _logger = logger;
            _databaseConnectionFactory = databaseConnectionFactory;
        }

        public async Task<int> GetUpdateId(string meetingId, string caseNumber)
        {
            var sqlQuery = @"
                select
	                meeting_seat_updates.id,
	                meeting_seat_updates.attendees_eventid,
	                meeting_events.case_number
                from
                    meeting_seat_updates 
	            join
                    meeting_events on meeting_seat_updates.attendees_eventid = meeting_events.event_id
                where
                    meeting_seat_updates.meeting_id = @meetingId and meeting_events.case_number = @caseNumber
                order by
                    meeting_seat_updates.timestamp desc
            ";

            using var dbConnection = await _databaseConnectionFactory.CreateOpenConnection();
            return (await dbConnection.QueryAsync<int>(sqlQuery, new { meetingId, caseNumber })).FirstOrDefault();
        }

        public async Task<List<MeetingSeat>> GetSeats(int updateId)
        {
            var sqlQuery = @"
                select
                    person_fi,
                    person_sv,
                    seat_id
                from
                    meeting_seats
                where
                    meeting_seat_update_id = @updateId
            ";

            using var dbConnection = await _databaseConnectionFactory.CreateOpenConnection();
            
            return (await dbConnection.QueryAsync<MeetingSeat>(sqlQuery, new { updateId })).ToList();
        }

        public async Task InsertMeetingSeatUpdate(MeetingSeatUpdate meetingSeatUpdate, List<MeetingSeat> meetingSeats, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"insert into meeting_seat_updates (meeting_id, attendees_eventid, sequence_number, timestamp) values (
                @meetingId,
                @eventId,
                @sequenceNumber,
                @timestamp
               )";
            sqlQuery += " returning id as Id;";

            var rowId = (await connection.QueryAsync<RowId>(sqlQuery, meetingSeatUpdate, transaction)).First();
            await InsertMeetingSeats(meetingSeats, rowId.Id, connection, transaction);
        }

        private Task InsertMeetingSeats(List<MeetingSeat> meetingSeats, int updateId, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Executing UpsertMeetingSeats()");
            var sqlQuery = @"insert into meeting_seats (meeting_seat_update_id, seat_id, person_fi, person_sv) values (
                @meetingSeatUpdateId,
                @seatId,
                @personFi,
                @personSv
            )";

            return connection.ExecuteAsync(sqlQuery, meetingSeats.Select(item => new
            {
                meetingSeatUpdateId = updateId,
                seatId = item.SeatID,
                personFi = item.PersonFI,
                personSv = item.PersonSV
            }), transaction);
        }

        private class RowId
        {
            public int Id { get; set; }
        }
    }
}