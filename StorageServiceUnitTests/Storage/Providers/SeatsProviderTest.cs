using Microsoft.Extensions.Logging;
using Moq;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Providers;
using Storage.Repositories;
using Storage.Repositories.Models;
using Xunit;

namespace StorageServiceUnitTests.Storage.Providers
{
    public class SeatsProviderTest
    {
        private readonly Mock<ILogger<SeatsProvider>> _logger;
        private readonly Mock<IMeetingSeatsRepository> _meetingSeatsRepository;
        private readonly Mock<IVotingsRepository> _votingsRepository;
        private readonly SeatsProvider _seatsProvider;
        private readonly string meetingId = "meetingA";
        private readonly string caseNumber = "123";

        public SeatsProviderTest()
        {
            _logger = new Mock<ILogger<SeatsProvider>>();
            _meetingSeatsRepository = new Mock<IMeetingSeatsRepository>();
            _votingsRepository = new Mock<IVotingsRepository>();
            _seatsProvider = new SeatsProvider(_logger.Object, _meetingSeatsRepository.Object, _votingsRepository.Object);
        }
        
        [Fact]
        public async Task GetSeats_WithMultipleVotings_ReturnsSequentialVotingNumbers()
        {
            // Arrange
            List<VotingEvent> votingEvents = new()
            {
                new VotingEvent { VotingNumber = 1 },
                new VotingEvent { VotingNumber = 2 }
            };

            List<MeetingSeat> seatsForVoting1 = new() { new MeetingSeat { SeatID = "seat1" } };
            List<MeetingSeat> seatsForVoting2 = new() { new MeetingSeat { SeatID = "seat2" } };

            _votingsRepository.Setup(x => x.GetVoting(meetingId, caseNumber)).ReturnsAsync(votingEvents);
            _meetingSeatsRepository.Setup(x => x.GetUpdateIdForVoting(meetingId, 1)).ReturnsAsync(3);
            _meetingSeatsRepository.Setup(x => x.GetSeats(3)).ReturnsAsync(seatsForVoting1);
            _meetingSeatsRepository.Setup(x => x.GetUpdateIdForVoting(meetingId, 2)).ReturnsAsync(4);
            _meetingSeatsRepository.Setup(x => x.GetSeats(4)).ReturnsAsync(seatsForVoting2);

            // Act
            var result = await _seatsProvider.GetSeats(meetingId, caseNumber);

            // Assert
            _votingsRepository.Verify(x => x.GetVoting(meetingId, caseNumber), Times.Once);
            _meetingSeatsRepository.Verify(x => x.GetUpdateIdForVoting(meetingId, 1), Times.Once);
            _meetingSeatsRepository.Verify(x => x.GetSeats(3), Times.Once);
            _meetingSeatsRepository.Verify(x => x.GetUpdateIdForVoting(meetingId, 2), Times.Once);
            _meetingSeatsRepository.Verify(x => x.GetSeats(4), Times.Once);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].VotingNumber);
            Assert.Single(result[0].Seats);
            Assert.Equal("seat1", result[0].Seats[0].SeatId);

            Assert.Equal(2, result[1].VotingNumber);
            Assert.Single(result[1].Seats);
            Assert.Equal("seat2", result[1].Seats[0].SeatId);
        }

        [Fact]
        public async Task GetSeats_WithSingleVoting_ReturnsActualVotingNumber()
        {
            // Arrange
            List<VotingEvent> votingEvents = new()
            {
                new VotingEvent { VotingNumber = 12 }
            };

            List<MeetingSeat> seatsForVoting = new() { new MeetingSeat { SeatID = "single_seat" } };

            _votingsRepository.Setup(x => x.GetVoting(meetingId, caseNumber)).ReturnsAsync(votingEvents);
            _meetingSeatsRepository.Setup(x => x.GetUpdateIdForVoting(meetingId, 12)).ReturnsAsync(55);
            _meetingSeatsRepository.Setup(x => x.GetSeats(55)).ReturnsAsync(seatsForVoting);

            // Act
            var result = await _seatsProvider.GetSeats(meetingId, caseNumber);

            // Assert
            _votingsRepository.Verify(x => x.GetVoting(meetingId, caseNumber), Times.Once);
            _meetingSeatsRepository.Verify(x => x.GetUpdateIdForVoting(meetingId, 12), Times.Once);
            _meetingSeatsRepository.Verify(x => x.GetSeats(55), Times.Once);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(12, result[0].VotingNumber); // Returns actual database voting number
            Assert.Single(result[0].Seats);
            Assert.Equal("single_seat", result[0].Seats[0].SeatId);
        }

        [Fact]
        public async Task GetSeats_NoVotings_FallsbackToLatestCaseSeats()
        {
            // Arrange
            _votingsRepository.Setup(x => x.GetVoting(meetingId, caseNumber)).ReturnsAsync(new List<VotingEvent>());
            _meetingSeatsRepository.Setup(x => x.GetUpdateId(meetingId, caseNumber)).ReturnsAsync(9);
            _meetingSeatsRepository.Setup(x => x.GetSeats(9)).ReturnsAsync(new List<MeetingSeat> { new MeetingSeat { SeatID = "fallback" } });

            // Act
            var result = await _seatsProvider.GetSeats(meetingId, caseNumber);

            // Assert
            _votingsRepository.Verify(x => x.GetVoting(meetingId, caseNumber), Times.Once);
            _meetingSeatsRepository.Verify(x => x.GetUpdateId(meetingId, caseNumber), Times.Once);
            _meetingSeatsRepository.Verify(x => x.GetSeats(9), Times.Once);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(0, result[0].VotingNumber);
            Assert.Single(result[0].Seats);
            Assert.Equal("fallback", result[0].Seats[0].SeatId);
        }
    }
}
