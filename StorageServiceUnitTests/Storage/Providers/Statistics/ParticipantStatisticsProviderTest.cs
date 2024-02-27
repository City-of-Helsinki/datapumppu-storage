using Microsoft.Extensions.Logging;
using Storage.Providers.Statistics;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Providers.Statistics
{
    public class ParticipantStatisticsProviderTest
    {
        private readonly Mock<IParticipantsRepository> _participantsRepository;
        private readonly Mock<ILogger<ParticipantStatisticsProvider>> _logger;
        private readonly Mock<IAgendaItemsRepository> _agendaItemsRepository;
        private readonly Mock<IMeetingSeatsRepository> _meetingSeatsRepository;
        private readonly ParticipantStatisticsProvider _participantsStatisticsProvider;
        private readonly int year = 2023;

        public ParticipantStatisticsProviderTest()
        {
            _agendaItemsRepository = new Mock<IAgendaItemsRepository>();
            _logger = new Mock<ILogger<ParticipantStatisticsProvider>>();
            _meetingSeatsRepository = new Mock<IMeetingSeatsRepository>();
            _participantsRepository = new Mock<IParticipantsRepository>();
            _participantsStatisticsProvider = new ParticipantStatisticsProvider(
                _logger.Object,
                _participantsRepository.Object,
                _agendaItemsRepository.Object,
                _meetingSeatsRepository.Object);
        }

        [Fact]
        public async void GetStatistics_ReturnsExpectedData()
        {
            List<string> persons = new()
            {
                "personA",
                "personB",
            };

            List<AgendaItem> agendaItems = new()
            {
                new()
                {
                    MeetingID = "meetingA",
                    AgendaPoint = 2
                },
                new AgendaItem(),
            };

            List<MeetingSeat> meetingSeats = new()
            {
                new MeetingSeat(),
                new MeetingSeat(),
            };

            _participantsRepository.Setup(x => x.GetParticipants(year)).Returns(Task.FromResult(persons));
            _agendaItemsRepository.Setup(x => x.FetchAgendasByYear(year)).Returns(Task.FromResult(agendaItems));
            _meetingSeatsRepository.Setup(x => x.GetSeats(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(meetingSeats));


            var result = await _participantsStatisticsProvider.GetStatistics(year);

            Assert.NotNull(result);
            Assert.IsType<List<ParticipationsPersonDTO>>(result);
        }
    }
}