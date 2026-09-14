using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Mappers;
using Storage.Providers.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Collections.Generic;

namespace Storage.Providers
{
    /// <summary>
    /// Provides business logic for retrieving voting information and individual votes.
    /// Aggregates voting events with their associated vote details.
    /// </summary>
    public interface IVotesProvider
    {
        /// <summary>
        /// Retrieves all voting events and their individual votes for a specific meeting and case.
        /// </summary>
        /// <param name="meetingId">The unique meeting identifier.</param>
        /// <param name="caseNumber">The case number within the meeting.</param>
        /// <returns>A list of WebApiVotingDTO containing voting details and individual vote breakdowns.</returns>
        Task<List<WebApiVotingDTO>> GetVoting(string meetingId, string caseNumber);
    }

    /// <summary>
    /// Implementation of IVotesProvider that retrieves voting events and aggregates individual votes.
    /// Maps bilingual voting information (Finnish and Swedish) to API DTOs.
    /// </summary>
    public class VotesProvider : IVotesProvider
    {
        private readonly ILogger<VotesProvider> _logger;
        private readonly IVotingsRepository _votingsRepository;

        /// <summary>
        /// Initializes a new instance of the VotesProvider class.
        /// </summary>
        /// <param name="logger">The logger for diagnostic information.</param>
        /// <param name="votingsRepository">The repository for accessing voting data.</param>
        public VotesProvider(
            ILogger<VotesProvider> logger,
            IVotingsRepository votingsRepository)
        {
            _logger = logger;
            _votingsRepository = votingsRepository;
        }

        public async Task<List<WebApiVotingDTO>> GetVoting(string meetingId, string caseNumber)
        {
            var votingList = await _votingsRepository.GetVoting(meetingId, caseNumber);

            var list = new List<WebApiVotingDTO>();
            foreach (var voting in votingList)
            {
                var votes = await _votingsRepository.GetVotes(meetingId, voting.VotingNumber);
                list.Add(MapVotingToDTO(voting, votes));
            }

            return list;
        }

        private WebApiVotingDTO MapVotingToDTO(VotingEvent voting, List<Vote> votes)
        {
            return new WebApiVotingDTO
            {
                AbsentCount = voting.VotesAbsent ?? 0,
                EmptyCount = voting.VotesEmpty ?? 0,
                ForCount = voting.VotesFor ?? 0,
                AgainstCount = voting.VotesAgainst ?? 0,
                ForTitleFI = voting.ForTitleFI,
                ForTitleSV = voting.ForTitleSV,
                AgainstTitleFI = voting.AgainstTitleFI,
                AgainstTitleSV = voting.AgainstTitleSV,
                ForTextFI = voting.ForTextFI,
                ForTextSV = voting.ForTextSV,
                AgainstTextFI = voting.AgainstTextFI,
                AgainstTextSV = voting.AgainstTextSV,
                Votes = votes.Select(vote =>
                {
                    return new WebApiVoteDTO
                    {
                        Name = vote.Person,
                        VoteType = (int)vote.VoteType
                    };
                }).ToArray()
            };
        }
    }
}
