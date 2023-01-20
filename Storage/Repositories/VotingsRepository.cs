using System.Data;
using System.Text.Json;
using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;

namespace Storage.Repositories
{
    public interface IVotingsRepository
    {
        Task UpsertVotingStartedEvent(VotingEvent votingEvent, IDbConnection connection, IDbTransaction transaction);

        Task SaveVotingResult(VotingEvent votingEvent, IDbConnection connection, IDbTransaction transaction);

        Task<VotingEvent?> GetVoting(string meetingId, string caseId);

        Task<List<Vote>> GetVotes(string meetingId, int votingNumber);
    }

    public class VotingsRepository : IVotingsRepository
    {
        private readonly ILogger<VotingsRepository> _logger;
        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;

        public VotingsRepository(ILogger<VotingsRepository> logger,
            IDatabaseConnectionFactory databaseConnectionFactory)
        {
            _logger = logger;
            _databaseConnectionFactory = databaseConnectionFactory;
        }

        public async Task<VotingEvent?> GetVoting(string meetingId, string caseId)
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
                    against_title_fi,
                    for_title_sv,
                    against_title_sv
                from votings
                join meeting_events on votings.voting_ended_eventid = meeting_events.event_id
                where
                    meeting_events.meeting_id = @meetingId and meeting_events.case_number = @caseId
            ";

            using var connection = await _databaseConnectionFactory.CreateOpenConnection();
            return (await connection.QueryAsync<VotingEvent>(sqlQuery, new { meetingId, caseId })).FirstOrDefault();
        }

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

        public async Task SaveVotingResult(VotingEvent votingEvent, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation(("Executing SaveVotingResult()"));
            await UpsertVoting(votingEvent, connection, transaction);
            await InsertVotes(votingEvent.Votes, connection, transaction);
        }

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
