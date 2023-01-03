using Dapper;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Repositories
{
    public interface IMeetingSeatsRepository
    {
        Task InsertMeetingSeatUpdate(MeetingSeatUpdate meetingSeatUpdate, List<MeetingSeat> meetingSeats, IDbConnection connection, IDbTransaction transaction);
    }

    public class MeetingSeatsRepository : IMeetingSeatsRepository
    {
        private readonly ILogger<MeetingSeatsRepository> _logger;

        public MeetingSeatsRepository(ILogger<MeetingSeatsRepository> logger)
        {
            _logger = logger;
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
            var sqlQuery = @"insert into meeting_seats (meeting_seat_update_id, seat_id, person, additional_info_fi, additional_info_sv) values (
                @meetingSeatUpdateId,
                @seatId,
                @person,
                @additionalInfoFi,
                @additionalInfoSv
            )";

            return connection.ExecuteAsync(sqlQuery, meetingSeats.Select(item => new
            {
                meetingSeatUpdateId = updateId,
                seatId = item.SeatID,
                person = item.Person,
                additionalInfoFi = item.AdditionalInfoFI,
                additionalInfoSv = item.AdditionalInfoSV
            }), transaction);
        }

        private class RowId
        {
            public int Id { get; set; }
        }
    }
}