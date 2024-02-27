using Storage.Providers.Statistics;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories.Models.Statistics;
using Storage.Repositories.Statistics;

namespace StorageServiceUnitTests.Storage.Providers.Statistics
{
    public class VotingStatisticsProviderTest
    {
        private readonly Mock<IVotingStatisticsRepository> _votingStatisticsRepository;
        private readonly VotingStatisticsProvider _votingStatisticsProvider;
        private readonly int year = 2023;

        public VotingStatisticsProviderTest()
        {
            _votingStatisticsRepository = new Mock<IVotingStatisticsRepository>();
            _votingStatisticsProvider = new VotingStatisticsProvider(
                _votingStatisticsRepository.Object
            );
        }

        [Fact]
        public async void GetStatementStatistics_ReturnsExpectedData()
        {
            List<VotingStatistics> votingStatistics = new()
            {
                new VotingStatistics(),
                new VotingStatistics(),
            };

            _votingStatisticsRepository.Setup(x => x.GetStatistics(year)).Returns(Task.FromResult(votingStatistics));

            var result = await _votingStatisticsProvider.GetStatistics(year);

            Assert.NotNull(result);
            Assert.IsType<List<VotingStatisticsDTO>>(result);
        }
    }
}