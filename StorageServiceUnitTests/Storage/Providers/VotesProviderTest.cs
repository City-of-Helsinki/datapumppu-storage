
using Microsoft.Extensions.Logging;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Providers;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Providers
{
    public class VotesProviderTest
    {
        private readonly Mock<ILogger<VotesProvider>> _logger;
        private readonly Mock<IVotingsRepository> _votingsRepository;
        private readonly VotesProvider _votesProvider;
        private readonly string meetingId = "meetingA";
        private readonly string caseNumber = "123";

        public VotesProviderTest()
        {
            _logger = new Mock<ILogger<VotesProvider>>();
            _votingsRepository = new Mock<IVotingsRepository>();
            _votesProvider = new VotesProvider(_logger.Object, _votingsRepository.Object);
        }
        [Fact]
        public async void GetVoting_ReturnsExpectedData()
        {
            List<VotingEvent> votingEvents = new()
            {
                new VotingEvent
                {
                    VotingNumber = 1
                },
                new VotingEvent
                {
                    VotingNumber = 2
                },
            };

            List<Vote> votes = new()
            {
                new Vote(),
                new Vote(),
                new Vote(),
                new Vote(),
            };

            _votingsRepository.Setup(x => x.GetVoting(meetingId, caseNumber)).Returns(Task.FromResult(votingEvents));
            _votingsRepository.Setup(x => x.GetVotes(meetingId, It.IsAny<int>())).Returns(Task.FromResult(votes));

            var result = await _votesProvider.GetVoting(meetingId, caseNumber);

            _votingsRepository.Verify(x => x.GetVoting(meetingId, caseNumber), Times.Once);
            _votingsRepository.Verify(x => x.GetVotes(meetingId, It.IsAny<int>()), Times.Exactly(2));

            Assert.NotEmpty(result);
            Assert.IsType<List<WebApiVotingDTO>>(result);
        }
    }
}