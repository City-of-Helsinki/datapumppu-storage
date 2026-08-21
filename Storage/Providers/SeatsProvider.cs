using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Mappers;
using Storage.Providers.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace Storage.Providers
{
    /// <summary>
    /// Provides business logic for retrieving meeting seat allocations.
    /// Maps seat assignments to API-ready DTOs for participant positioning.
    /// </summary>
    public interface ISeatsProvider
    {
        /// <summary>
        /// Retrieves seat allocations grouped by voting number for a specific meeting and case.
        /// </summary>
        /// <param name="meetingId">The unique meeting identifier.</param>
        /// <param name="caseNumber">The case number within the meeting.</param>
        /// <returns>A list of WebApiSeatsDTO containing seat allocations grouped by voting number.</returns>
        Task<List<WebApiSeatsDTO>> GetSeats(string meetingId, string caseNumber);
    }

    public class SeatsProvider : ISeatsProvider
    {
        private readonly ILogger<SeatsProvider> _logger;
        private readonly IMeetingSeatsRepository _meetingSeatsRepository;
        private readonly IVotingsRepository _votingsRepository;

        public SeatsProvider(
            ILogger<SeatsProvider> logger,
            IMeetingSeatsRepository meetingSeatsRepository,
            IVotingsRepository votingsRepository)
        {
            _logger = logger;
            _meetingSeatsRepository = meetingSeatsRepository;
            _votingsRepository = votingsRepository;
        }

        public async Task<List<WebApiSeatsDTO>> GetSeats(string meetingId, string caseNumber)
        {
            _logger.LogInformation("GetSeats for {0}, case {1}", meetingId, caseNumber);
            var votings = await _votingsRepository.GetVoting(meetingId, caseNumber);
            var result = new List<WebApiSeatsDTO>();

            foreach (var voting in votings)
            {
                var updateId = await _meetingSeatsRepository.GetUpdateIdForVoting(meetingId, voting.VotingNumber);
                var seats = await _meetingSeatsRepository.GetSeats(updateId);

                if (seats != null && seats.Any())
                {
                    result.Add(new WebApiSeatsDTO
                    {
                        VotingNumber = voting.VotingNumber,
                        Seats = seats.Select(MapSeatsToDTO).ToList()
                    });
                }
            }

            result = result.OrderBy(x => x.VotingNumber).ToList();

            if (!result.Any())
            {
                // Fallback to the latest seats for that case if no voting is present (VotingNumber = 0)
                var updateId = await _meetingSeatsRepository.GetUpdateId(meetingId, caseNumber);
                var seats = await _meetingSeatsRepository.GetSeats(updateId);
                if (seats != null && seats.Any())
                {
                    result.Add(new WebApiSeatsDTO
                    {
                        VotingNumber = 0,
                        Seats = seats.Select(MapSeatsToDTO).ToList()
                    });
                }
            }

            return result;
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

    }
}
