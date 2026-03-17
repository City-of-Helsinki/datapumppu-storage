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
        /// Retrieves seat allocations for a specific meeting and case.
        /// </summary>
        /// <param name="meetingId">The unique meeting identifier.</param>
        /// <param name="caseNumber">The case number within the meeting.</param>
        /// <returns>A list of WebApiSeatDTO containing seat allocation details.</returns>
        Task<List<WebApiSeatDTO>> GetSeats(string meetingId, string caseNumber);
    }

    public class SeatsProvider : ISeatsProvider
    {
        private readonly ILogger<SeatsProvider> _logger;
        private readonly IMeetingSeatsRepository _meetingSeatsRepository;

        public SeatsProvider(
            ILogger<SeatsProvider> logger,
            IMeetingSeatsRepository meetingSeatsRepository)
        {
            _logger = logger;
            _meetingSeatsRepository = meetingSeatsRepository;
        }

        public async Task<List<WebApiSeatDTO>> GetSeats(string meetingId, string caseNumber)
        {
            var updateId = await _meetingSeatsRepository.GetUpdateId(meetingId, caseNumber);
            var seats = await _meetingSeatsRepository.GetSeats(updateId);

            if (seats == null) 
            {
                return new List<WebApiSeatDTO>();
            }

            return seats.Select(MapSeatsToDTO).ToList();
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
