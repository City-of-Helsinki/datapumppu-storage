using Microsoft.Extensions.Logging;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Providers;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Providers
{
    public class SeatsProviderTest
    {
        private readonly Mock<ILogger<SeatsProvider>> _logger;
        private readonly Mock<IMeetingSeatsRepository> _meetingSeatsRepository;
        private readonly SeatsProvider _seatsProvider;
        private readonly string meetingId = "meetingA";
        private readonly string caseNumber = "123";

        public SeatsProviderTest()
        {
            _logger = new Mock<ILogger<SeatsProvider>>();
            _meetingSeatsRepository = new Mock<IMeetingSeatsRepository>();
            _seatsProvider = new SeatsProvider(_logger.Object, _meetingSeatsRepository.Object);
        }
        
        [Fact]
        public async void GetSeats_ReturnsExpectedData()
        {
            List<MeetingSeat> meetingSeats = new()
            {
                new MeetingSeat(),
                new MeetingSeat(),
                new MeetingSeat(),
            };

            _meetingSeatsRepository.Setup(x => x.GetUpdateId(meetingId, caseNumber)).Returns(Task.FromResult(3));
            _meetingSeatsRepository.Setup(x => x.GetSeats(3)).Returns(Task.FromResult(meetingSeats));

            var result = await _seatsProvider.GetSeats(meetingId, caseNumber);

            _meetingSeatsRepository.Verify(x => x.GetUpdateId(meetingId, caseNumber), Times.Once);
            _meetingSeatsRepository.Verify(x => x.GetSeats(3), Times.Once);

            Assert.NotNull(result);
            Assert.IsType<List<WebApiSeatDTO>>(result);
        }
    }
}