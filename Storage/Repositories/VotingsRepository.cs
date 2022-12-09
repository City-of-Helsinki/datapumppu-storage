using System.Data;
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
            var sqlQuery = @"insert into votings (meeting_id, voting_id, voting_started, voting_started_eventid, voting_type, voting_type_text, for_text, for_title, against_text, against_title) values (
                @meetingId,
                @votingId,
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
            await UpdateVoting(votingEvent, connection, transaction);
            await InsertVotes(votingEvent.Votes, connection, transaction);
        }

        private Task UpdateVoting(VotingEvent votingEvent, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = $@"update votings set
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
                where voting_id = @votingId
            ";

            return connection.ExecuteAsync(sqlQuery, votingEvent, transaction);
        }

        private Task InsertVotes(List<Vote> votes, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"insert into votes (voting_id, voter_name, vote_type) values (
                @votingId,
                @voterName,
                @voteType
            )";

            return connection.ExecuteAsync(sqlQuery, votes.Select(item => new
            {
                votingId = item.VotingID,
                voterName = item.VoterName,
                voteType = item.VoteType
            }), transaction);
        }
    }
}
