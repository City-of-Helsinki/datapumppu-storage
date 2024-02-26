
using Microsoft.Extensions.Logging;
using Storage.Providers;
using Storage.Providers.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Providers
{
    public class ReservationsProviderTests
    {
        private readonly Mock<IStatementsRepository> _statementsRepository;
        private readonly Mock<ILogger<ReservationsProvider>> _logger;
        private readonly Mock<IEventsRepository> _eventsRepository;
        private readonly ReservationsProvider _reservationsProvider;
        private readonly string meetingId = "meetingA";
        private readonly string caseNumber = "123";

        public ReservationsProviderTests()
        {
            _statementsRepository = new Mock<IStatementsRepository>();
            _logger = new Mock<ILogger<ReservationsProvider>>();
            _eventsRepository = new Mock<IEventsRepository>();
            _reservationsProvider = new ReservationsProvider(_logger.Object,
                _statementsRepository.Object,
                _eventsRepository.Object);
        }
        [Fact]
        public async void GetReservations_ReturnsExpectedData()
        {
            List<StatementReservation> statementReservations = new()
            {
                new StatementReservation(),
                new StatementReservation(),
            };

            List<ReplyReservation> replyReservations = new()
            {
                new ReplyReservation(),
                new ReplyReservation(),
            };

            ReplyReservation replyReservation = new()
            {
                MeetingID = "meetingA",
                Person = "personA",
                EventID = new Guid(),
            };

            _eventsRepository.Setup(x => x.IsAgendaPointHandled(meetingId, caseNumber)).Returns(Task.FromResult(true));
            _statementsRepository.Setup(x => x.GetStatementReservations(meetingId, caseNumber)).Returns(Task.FromResult(statementReservations));
            _statementsRepository.Setup(x => x.GetReplyReservations(meetingId, caseNumber)).Returns(Task.FromResult(replyReservations));
            _statementsRepository.Setup(x => x.GetActiveSpeaker(meetingId, caseNumber)).Returns(Task.FromResult(replyReservation));

            var result = await _reservationsProvider.GetReservations(meetingId, caseNumber);

            _eventsRepository.Verify(x => x.IsAgendaPointHandled(meetingId, caseNumber), Times.Once);
            _statementsRepository.Verify(x => x.GetStatementReservations(meetingId, caseNumber), Times.Once);
            _statementsRepository.Verify(x => x.GetReplyReservations(meetingId, caseNumber), Times.Once);
            _statementsRepository.Verify(x => x.GetActiveSpeaker(meetingId, caseNumber), Times.Once);

            Assert.NotNull(result);
            Assert.IsType<List<WebApiReservationDTO>>(result);
        }
    }
}