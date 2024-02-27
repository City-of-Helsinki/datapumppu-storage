using Storage.Providers.Statistics;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories.Models.Statistics;
using Storage.Repositories.Statistics;

namespace StorageServiceUnitTests.Storage.Providers.Statistics
{
    public class PersonStatementStatisticsTest
    {
        private readonly Mock<IPersonStatementStatisticsRepository> _personStatementStatisticsRepositoy;
        private readonly PersonStatementStatisticsProvider _personStatementStatisticsProvider;
        private readonly int year = 2023;

        public PersonStatementStatisticsTest()
        {
            _personStatementStatisticsRepositoy = new Mock<IPersonStatementStatisticsRepository>();
            _personStatementStatisticsProvider = new PersonStatementStatisticsProvider(
                _personStatementStatisticsRepositoy.Object
            );
        }

        [Fact]
        public async void GetStatementStatistics_ReturnsExpectedData()
        {
            List<PersonStatementStatistics> personStatementStatistics = new()
            {
                new PersonStatementStatistics(),
                new PersonStatementStatistics(),
            };

            _personStatementStatisticsRepositoy.Setup(x => x.GetStatistics(year)).Returns(Task.FromResult(personStatementStatistics));

            var result = await _personStatementStatisticsProvider.GetStatementStatistics(year);

            Assert.NotNull(result);
            Assert.IsType<List<PersonStatementStatisticsDTO>>(result);
        }
    }
}