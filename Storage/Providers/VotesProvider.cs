using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Mappers;
using Storage.Providers.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace Storage.Providers
{
    public interface IVotesProvider
    {
        Task<WebApiVotesDTO?> GetVoting(string meetingId, string caseNumber);
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

        public async Task<WebApiVotesDTO?> GetVoting(string meetingId, string caseNumber)
        {
            var voting = await _votingsRepository.GetVoting(meetingId, caseNumber);
            if (voting == null)
            {
                return null;
            }

            var votes = await _votingsRepository.GetVotes(meetingId, voting.VotingNumber);

            return MapVotingToDTO(voting, votes);
        }

        private WebApiSeatDTO MapSeatsToDTO(MeetingSeat seat)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MeetingSeat, WebApiSeatDTO>();
            });
            config.AssertConfigurationIsValid();

            return config.CreateMapper().Map<WebApiSeatDTO>(seat);
        }

        private WebApiVotesDTO MapVotingToDTO(VotingEvent voting, List<Vote> votes)
        {
            return new WebApiVotesDTO
            {
                AbsentCount = voting.VotesAbsent ?? 0,
                EmptyCount = voting.VotesEmpty ?? 0,
                ForCount = voting.VotesFor ?? 0,
                AgainstCount = voting.VotesAgainst ?? 0,
                ForTitleFI = voting.ForTitleFI,
                ForTitleSV = voting.ForTitleSV,
                AgainstTitleFI = voting.AgainstTitleFI,
                AgainstTitleSV = voting.AgainstTitleSV,
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
