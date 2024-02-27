using Storage.Providers.Statistics;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories.Models.Statistics;
using Storage.Repositories.Statistics;

namespace StorageServiceUnitTests.Storage.Providers.Statistics
{
    public class StatementStatisticsProviderTest
    {
        private readonly Mock<IStatementStatisticsRepository> _statementStatisticsRepository;
        private readonly StatementStatisticsProvider _statementStatisticsProvider;
        private readonly int year = 2023;

        public StatementStatisticsProviderTest()
        {
            _statementStatisticsRepository = new Mock<IStatementStatisticsRepository>();
            _statementStatisticsProvider = new StatementStatisticsProvider(
                _statementStatisticsRepository.Object
            );
        }

        [Fact]
        public async void GetStatementStatistics_ReturnsExpectedData()
        {
            List<StatementStatistics> statementStatistics = new()
            {
                new StatementStatistics(),
                new StatementStatistics(),
            };

            _statementStatisticsRepository.Setup(x => x.GetStatistics(year)).Returns(Task.FromResult(statementStatistics));

            var result = await _statementStatisticsProvider.GetStatementStatistics(year);

            Assert.NotNull(result);
            Assert.IsType<List<StatementStatisticsDTO>>(result);
        }
    }
}