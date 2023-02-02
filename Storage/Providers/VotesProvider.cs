using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Mappers;
using Storage.Providers.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Collections.Generic;

namespace Storage.Providers
{
    public interface IVotesProvider
    {
        Task<List<WebApiVotingDTO>> GetVoting(string meetingId, string caseNumber);
    }

    public class VotesProvider : IVotesProvider
    {
        private readonly ILogger<VotesProvider> _logger;
        private readonly IVotingsRepository _votingsRepository;

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
