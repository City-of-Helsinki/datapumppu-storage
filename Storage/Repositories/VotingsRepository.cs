using System.Data;
using System.Text.Json;
using Dapper;
using Storage.Repositories.Models;

namespace Storage.Repositories
{
    public interface IVotingsRepository
    {
        Task InsertVoting(VotingEvent votingEvent, IDbConnection connection, IDbTransaction transaction);

        Task SaveVotingResult(VotingEvent votingEvent, IDbConnection connection, IDbTransaction transaction);
    }

    public class VotingsRepository : IVotingsRepository
    {
        private readonly ILogger<VotingsRepository> _logger;

        public VotingsRepository(ILogger<VotingsRepository> logger)
        {
            _logger = logger;
        }

        public Task InsertVoting(VotingEvent votingEvent, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Executing InsertVoting()");
            var sqlQuery = @"insert into votings (meeting_id, voting_number, voting_started, voting_started_eventid, voting_type, voting_type_text, for_text, for_title, against_text, against_title) values (
                @meetingId,
                @votingNumber,
                @timestamp,
                @eventId,
                @votingType,
                @votingTypeText,
                @forText,
                @forTitle,
                @againstText,
                @againstTitle 
            )";

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
            var sqlQuery = @"INSERT INTO votings (meeting_id, voting_number, voting_ended, voting_ended_eventid, voting_type, voting_type_text, votes_for, 
                votes_against, votes_empty, votes_absent, for_text, for_title, against_text, against_title) values(
                @meetingId, 
                @votingNumber,
                @timestamp,
                @eventId,
                @votingType,
                @votingTypeText,
                @votesFor,
                @votesAgainst,
                @votesEmpty,
                @votesAbsent,
                @forText,
                @forTitle,
                @againstText,
                @againstTitle) ";
            sqlQuery += $@"ON CONFLICT (meeting_id, voting_number) DO UPDATE SET
                voting_ended = @timestamp,
                voting_ended_eventid = @eventId,
                voting_type = @votingType,
                voting_type_text = @votingTypeText,
                votes_for = @votesFor,
                votes_against = @votesAgainst,
                votes_empty = @votesEmpty,
                votes_absent = @votesAbsent,
                for_text = @forText,
                for_title = @forTitle,
                against_text = @againstText,
                against_title = @againstTitle
                where votings.meeting_id = @meetingId AND votings.voting_number = @votingNumber
            ";

            return connection.ExecuteAsync(sqlQuery, votingEvent, transaction);
        }

        private Task InsertVotes(List<Vote> votes, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"insert into votes (meeting_id, voting_number, voter_name, vote_type) values (
                @meetingId,
                @votingNumber,
                @voterName,
                @voteType
            )";

            return connection.ExecuteAsync(sqlQuery, votes.Select(item => new
            {
                meetingId = item.MeetingID,
                votingNumber = item.VotingNumber,
                voterName = item.VoterName,
                voteType = item.VoteType
            }), transaction);
        }
    }
}
