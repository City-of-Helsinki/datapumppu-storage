using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;
using System.Data;
using System.Net.WebSockets;
using System.Transactions;

namespace Storage.Repositories
{
    /// <summary>
    /// Provides data access methods for meeting seat allocation management.
    /// </summary>
    public interface IMeetingSeatsRepository
    {
        /// <summary>
        /// Inserts a meeting seat update event with the associated seat allocations.
        /// </summary>
        /// <param name="meetingSeatUpdate">The seat update event information.</param>
        /// <param name="meetingSeats">The list of seat allocations for this update.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InsertMeetingSeatUpdate(MeetingSeatUpdate meetingSeatUpdate, List<MeetingSeat> meetingSeats, IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Retrieves the most recent seat update ID for a specific case number.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="caseNumber">The case number to find the latest seat update for.</param>
        /// <returns>The update ID of the most recent seat allocation.</returns>
        Task<int> GetUpdateId(string meetingId, string caseNumber);

        /// <summary>
        /// Retrieves all seats for a specific seat update.
        /// </summary>
        /// <param name="updateId">The seat update identifier.</param>
        /// <returns>A list of meeting seats with person and position information.</returns>
        Task<List<MeetingSeat>> GetSeats(int updateId);

        /// <summary>
        /// Retrieves all seats for a specific meeting and case number.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="caseNumber">The case number.</param>
        /// <returns>A list of meeting seats for the specified case.</returns>
        Task<List<MeetingSeat>> GetSeats(string meetingId, string caseNumber);

        /// <summary>
        /// Retrieves the most recent seat update ID for a specific voting number on or before the voting started.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="votingNumber">The voting number within the meeting.</param>
        /// <returns>The update ID of the most recent seat allocation on or before the voting started.</returns>
        Task<int> GetUpdateIdForVoting(string meetingId, int votingNumber);

        /// <summary>
        /// Retrieves all seats for a specific meeting and voting number.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="votingNumber">The voting number within the meeting.</param>
        /// <returns>A list of meeting seats for the specified voting session.</returns>
        Task<List<MeetingSeat>> GetSeatsForVoting(string meetingId, int votingNumber);
    }

    /// <summary>
    /// Implements meeting seat allocation data access operations using Dapper for PostgreSQL queries.
    /// Manages seat arrangements and participant positions during meetings.
    /// </summary>
    public class MeetingSeatsRepository : IMeetingSeatsRepository
    {
        private readonly ILogger<MeetingSeatsRepository> _logger;
        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;

        /// <summary>
        /// Initializes a new instance of the MeetingSeatsRepository class.
        /// </summary>
        /// <param name="logger">Logger for diagnostic information.</param>
        /// <param name="databaseConnectionFactory">Factory for creating database connections.</param>
        public MeetingSeatsRepository(ILogger<MeetingSeatsRepository> logger,
            IDatabaseConnectionFactory databaseConnectionFactory)
        {
            _logger = logger;
            _databaseConnectionFactory = databaseConnectionFactory;
        }

        /// <summary>
        /// Retrieves the most recent seat update ID for a meeting up to and including the specified case number.
        /// Searches through all case numbers from 1 to the specified case number.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="caseNumber">The case number to search up to.</param>
        /// <returns>The ID of the most recent seat update, or 0 if none found.</returns>
        public async Task<int> GetUpdateId(string meetingId, string caseNumber)
        {
            List<string> caseNumbers = new List<string>();
            for (int i = 1; i <= Int32.Parse(caseNumber); i++)
            {
                caseNumbers.Add(i.ToString());
            }

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
                    meeting_seat_updates.meeting_id = @meetingId and meeting_events.case_number = any (@caseNumbers)
                order by
                    meeting_seat_updates.timestamp desc, id desc
            ";

            using var dbConnection = await _databaseConnectionFactory.CreateOpenConnection();
            return (await dbConnection.QueryAsync<int>(sqlQuery, new { meetingId, caseNumbers    })).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves all seat allocations for a specific seat update.
        /// </summary>
        /// <param name="updateId">The seat update identifier.</param>
        /// <returns>A list of seat allocations with person names, seat IDs, and bilingual additional information.</returns>
        public async Task<List<MeetingSeat>> GetSeats(int updateId)
        {
            var sqlQuery = @"
                select
                    person,
                    additional_info_fi,
                    additional_info_sv,
                    seat_id
                from
                    meeting_seats
                where
                    meeting_seat_update_id = @updateId
            ";

            using var dbConnection = await _databaseConnectionFactory.CreateOpenConnection();
            
            return (await dbConnection.QueryAsync<MeetingSeat>(sqlQuery, new { updateId })).ToList();
        }

        /// <summary>
        /// Retrieves seats for a meeting and case number by first finding the update ID, then fetching the seats.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="caseNumber">The case number.</param>
        /// <returns>A list of meeting seats for the specified case.</returns>
        public async Task<List<MeetingSeat>> GetSeats(string meetingId, string caseNumber)
        {
            var updateId = await GetUpdateId(meetingId, caseNumber);
            return await GetSeats(updateId);
        }

        /// <summary>
        /// Retrieves the most recent seat update ID for a specific voting number on or before the voting started.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="votingNumber">The voting number within the meeting.</param>
        /// <returns>The update ID of the most recent seat allocation, or 0 if none found.</returns>
        public async Task<int> GetUpdateIdForVoting(string meetingId, int votingNumber)
        {
            var sqlQuery = @"
                select msu.id 
                from meeting_seat_updates msu
                join meeting_events me_msu on msu.attendees_eventid = me_msu.event_id
                where msu.meeting_id = @meetingId
                  and me_msu.sequence_number <= (
                      select me_v.sequence_number 
                      from votings v
                      join meeting_events me_v on v.voting_started_eventid = me_v.event_id
                      where v.meeting_id = @meetingId and v.voting_number = @votingNumber
                  )
                order by me_msu.sequence_number desc, msu.id desc
                limit 1;
            ";

            using var dbConnection = await _databaseConnectionFactory.CreateOpenConnection();
            return (await dbConnection.QueryAsync<int>(sqlQuery, new { meetingId, votingNumber })).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves all seats for a specific meeting and voting number.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="votingNumber">The voting number within the meeting.</param>
        /// <returns>A list of meeting seats for the specified voting session.</returns>
        public async Task<List<MeetingSeat>> GetSeatsForVoting(string meetingId, int votingNumber)
        {
            var updateId = await GetUpdateIdForVoting(meetingId, votingNumber);
            return await GetSeats(updateId);
        }

        /// <summary>
        /// Inserts a meeting seat update and its associated seat allocations in a single transaction.
        /// Returns the generated update ID which is used to link the seats to the update.
        /// </summary>
        /// <param name="meetingSeatUpdate">The seat update event containing meeting ID, event ID, and timestamp.</param>
        /// <param name="meetingSeats">The list of seat allocations for this update.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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