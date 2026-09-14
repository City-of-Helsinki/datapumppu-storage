using System.Data;
using System.Text.Json;
using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;

namespace Storage.Repositories
{
    /// <summary>
    /// Provides data access methods for voting event and vote management.
    /// </summary>
    public interface IVotingsRepository
    {
        /// <summary>
        /// Inserts or updates voting start event information.
        /// </summary>
        /// <param name="votingEvent">The voting start event to upsert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpsertVotingStartedEvent(VotingEvent votingEvent, IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Saves voting results including vote counts and individual votes.
        /// </summary>
        /// <param name="votingEvent">The voting event with results and individual votes.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveVotingResult(VotingEvent votingEvent, IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Retrieves all voting events for a specific case in a meeting.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="caseId">The case identifier.</param>
        /// <returns>A list of voting events with vote counts and bilingual descriptions.</returns>
        Task<List<VotingEvent>> GetVoting(string meetingId, string caseId);

        /// <summary>
        /// Retrieves individual votes for a specific voting number in a meeting.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="votingNumber">The voting number within the meeting.</param>
        /// <returns>A list of votes with person names and vote types.</returns>
        Task<List<Vote>> GetVotes(string meetingId, int votingNumber);
    }

    /// <summary>
    /// Implements voting data access operations using Dapper for PostgreSQL queries.
    /// Manages voting events, results, and individual vote tracking with bilingual support.
    /// </summary>
    public class VotingsRepository : IVotingsRepository
    {
        private readonly ILogger<VotingsRepository> _logger;
        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;

        /// <summary>
        /// Initializes a new instance of the VotingsRepository class.
        /// </summary>
        /// <param name="logger">Logger for diagnostic information.</param>
        /// <param name="databaseConnectionFactory">Factory for creating database connections.</param>
        public VotingsRepository(ILogger<VotingsRepository> logger,
            IDatabaseConnectionFactory databaseConnectionFactory)
        {
            _logger = logger;
            _databaseConnectionFactory = databaseConnectionFactory;
        }

        /// <summary>
        /// Retrieves all voting events for a case, including vote counts and bilingual descriptions.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="caseId">The case identifier to filter by.</param>
        /// <returns>A list of voting events with counts for for, against, empty, and absent votes.</returns>
        public async Task<List<VotingEvent>> GetVoting(string meetingId, string caseId)
        {
            _logger.LogInformation("Executing GetVoting()");
            var sqlQuery = @"
                select
                    voting_number,
                    voting_type_text_fi,
                    voting_type_text_sv,
                    votes_for,
                    votes_against,
                    votes_empty,
                    votes_absent,
                    for_title_fi,
                    for_text_fi,
                    against_title_fi,
                    against_text_fi,
                    for_title_sv,
                    for_text_sv,
                    against_title_sv,
                    against_text_sv
                from votings
                join meeting_events on votings.voting_ended_eventid = meeting_events.event_id
                where
                    meeting_events.meeting_id = @meetingId and meeting_events.case_number = @caseId
            ";

            using var connection = await _databaseConnectionFactory.CreateOpenConnection();
            return (await connection.QueryAsync<VotingEvent>(sqlQuery, new { meetingId, caseId })).ToList();
        }

        /// <summary>
        /// Retrieves individual votes for a voting event.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="votingNumber">The voting number within the meeting.</param>
        /// <returns>A list of votes with person names, seat numbers, and vote types.</returns>
        public async Task<List<Vote>> GetVotes(string meetingId, int votingNumber)
        {
            _logger.LogInformation("Executing GetVotes()");
            var sqlQuery = @"
                select
                    person,
                    vote_type
                from
                    votes
                where meeting_id = @meetingId and voting_number = @votingNumber";

            using var connection = await _databaseConnectionFactory.CreateOpenConnection();
            var votes = await connection.QueryAsync<Vote>(sqlQuery, new { meetingId, votingNumber });
            return votes?.ToList() ?? new List<Vote>();
        }

        /// <summary>
        /// Inserts or updates a voting start event including voting type, title, and option.
        /// Updates existing entries based on meeting_id, voting_number, and case_id.
        /// </summary>
        /// <param name="votingEvent">The voting event with type, title, and option, in Finnish and Swedish.</param>
        /// <param name="connection">The database connection within a transaction context.</param>
        /// <param name="transaction">The active transaction for consistency.</param>
        public Task UpsertVotingStartedEvent(VotingEvent votingEvent, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Executing InsertVoting()");
            var sqlQuery = @"insert into votings (meeting_id, voting_number, voting_started, voting_started_eventid, voting_type, voting_type_text_fi, 
                voting_type_text_sv, for_text_fi, for_text_sv, for_title_fi, for_title_sv, against_text_fi, against_text_sv, against_title_fi, against_title_sv) values (
                @meetingId,
                @votingNumber,
                @timestamp,
                @eventId,
                @votingType,
                @votingTypeTextFi,
                @votingTypeTextSv,
                @forTextFi,
                @forTextSv,
                @forTitleFi,
                @forTitleSv,
                @againstTextFi,
                @againstTextSv,
                @againstTitleFi, 
                @againstTitleSv 
            ) ";
            sqlQuery += $@"ON CONFLICT (meeting_id, voting_number) DO UPDATE SET
                voting_started = @timestamp,
                voting_started_eventid = @eventId,
                voting_type = @votingType,
                voting_type_text_fi = @votingTypeTextFi,
                voting_type_text_sv = @votingTypeTextSv,
                for_text_fi = @forTextFi,
                for_text_sv = @forTextSv,
                for_title_fi = @forTitleFi,
                for_title_sv = @forTitleSv,
                against_text_fi = @againstTextFi,
                against_text_sv = @againstTextSv,
                against_title_fi = @againstTitleFi, 
                against_title_sv = @againstTitleSv 
                where votings.meeting_id = @meetingId AND votings.voting_number = @votingNumber
            ";

            return connection.ExecuteAsync(sqlQuery, votingEvent, transaction);
        }

        /// <summary>
        /// Saves complete voting results including vote counts and individual votes.
        /// Updates voting event with counts then inserts individual votes.
        /// </summary>
        /// <param name="votingEvent">The voting event with result counts and individual votes.</param>
        /// <param name="connection">The database connection within a transaction context.</param>
        /// <param name="transaction">The active transaction for consistency.</param>
        public async Task SaveVotingResult(VotingEvent votingEvent, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation(("Executing SaveVotingResult()"));
            await UpsertVoting(votingEvent, connection, transaction);
            await InsertVotes(votingEvent.Votes, connection, transaction);
        }

        /// <summary>
        /// Upserts voting event information including vote counts.
        /// Inserts or updates based on meeting_id and voting_number.
        /// </summary>
        /// <param name="votingEvent">The voting event with result counts.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The active transaction.</param>
        private Task UpsertVoting(VotingEvent votingEvent, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("event : " + JsonSerializer.Serialize(votingEvent));
            var sqlQuery = @"INSERT INTO votings (meeting_id, voting_number, voting_ended, voting_ended_eventid, voting_type, voting_type_text_fi, 
                voting_type_text_sv, votes_for, votes_against, votes_empty, votes_absent, for_text_fi, for_text_sv, for_title_fi, for_title_sv, 
                against_text_fi, against_text_sv, against_title_fi, against_title_sv) values(
                @meetingId, 
                @votingNumber,
                @timestamp,
                @eventId,
                @votingType,
                @votingTypeTextFi,
                @votingTypeTextSv,
                @votesFor,
                @votesAgainst,
                @votesEmpty,
                @votesAbsent,
                @forTextFi,
                @forTextSv,
                @forTitleFi,
                @forTitleSv,
                @againstTextFi,
                @againstTextSv,
                @againstTitleFi,
                @againstTitleSv) ";
            sqlQuery += $@"ON CONFLICT (meeting_id, voting_number) DO UPDATE SET
                voting_ended = @timestamp,
                voting_ended_eventid = @eventId,
                voting_type = @votingType,
                voting_type_text_fi = @votingTypeTextFi,
                voting_type_text_sv = @votingTypeTextSv,
                votes_for = @votesFor,
                votes_against = @votesAgainst,
                votes_empty = @votesEmpty,
                votes_absent = @votesAbsent,
                for_text_fi = @forTextFi,
                for_text_sv = @forTextSv,
                for_title_fi = @forTitleFi,
                for_title_sv = @forTitleSv,
                against_text_fi = @againstTextFi,
                against_text_sv = @againstTextSv,
                against_title_fi = @againstTitleFi,
                against_title_sv = @againstTitleSv
                where votings.meeting_id = @meetingId AND votings.voting_number = @votingNumber
            ";

            return connection.ExecuteAsync(sqlQuery, votingEvent, transaction);
        }

        /// <summary>
        /// Inserts individual votes in a batch operation.
        /// </summary>
        /// <param name="votes">The list of votes to insert.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The active transaction.</param>
        private Task InsertVotes(List<Vote> votes, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"insert into votes (meeting_id, voting_number, person, vote_type, additional_info_fi, additional_info_sv) values (
                @meetingId,
                @votingNumber,
                @person,
                @voteType,
                @additionalInfoFi,
                @additionalInfoSv
            )";

            return connection.ExecuteAsync(sqlQuery, votes.Select(item => new
            {
                meetingId = item.MeetingID,
                votingNumber = item.VotingNumber,
                person = item.Person,
                voteType = item.VoteType,
                additionalInfoFi = item.AdditionalInfoFI,
                additionalInfoSv = item.AdditionalInfoSV
            }), transaction);
        }
    }
}
