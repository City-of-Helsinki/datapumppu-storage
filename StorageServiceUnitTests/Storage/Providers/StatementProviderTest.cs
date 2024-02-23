
using AutoMapper;
using Microsoft.Extensions.Logging;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Providers;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Providers
{
    public class StatementProviderTests
    {
        private readonly Mock<IStatementsRepository> _statementsRepository;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IVideoSyncRepository> _videoSyncRepository;
        private readonly Mock<IMeetingsRepository> _meetingsRepository;
        private readonly Mock<ILogger<StatementProvider>> _logger;
        private readonly StatementProvider _statementProvider;
        private readonly string personName = "personA";
        private readonly string lang = "fi";
        private readonly int year = 2023;

        public StatementProviderTests()
        {
            _statementsRepository = new Mock<IStatementsRepository>();
            _videoSyncRepository = new Mock<IVideoSyncRepository>();
            _meetingsRepository = new Mock<IMeetingsRepository>();
            _logger = new Mock<ILogger<StatementProvider>>();
            _mapper = new Mock<IMapper>();
            _statementProvider = new StatementProvider(_logger.Object,
            _statementsRepository.Object,
            _videoSyncRepository.Object,
            _meetingsRepository.Object);
        }
        [Fact]
        public async void GetStatements_ReturnsExpectedData()
        {
            var expectedData = new WebApiStatementsDTO
            {
                Person = "personA",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow,
                SpeechType = 1,
                DurationSeconds = 54,
                AdditionalInfoFI = "",
                AdditionalInfoSV = "",
                Title = "Kohta 1",
                CaseNumber = 3,
                ItemNumber = "3",
                MeetingId = "meetingA",
                VideoPosition = 52,
                VideoLink = "videoLink"
            };

            _statementsRepository.Setup(x => x.GetStatements(expectedData.MeetingId, expectedData.CaseNumber.ToString())).Returns(Task.FromResult(new List<Statement>()));

            var result = await _statementProvider.GetStatements(expectedData.MeetingId, expectedData.CaseNumber.ToString());

            _statementsRepository.Verify(x => x.GetStatements(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.NotNull(result);
            Assert.IsType<List<WebApiStatementsDTO>>(result);
            Assert.IsAssignableFrom<List<WebApiStatementsDTO>>(result);
        }

        [Fact]
        public async void GetStatementsByPerson_ReturnsExpectedData()
        {
            var statements = new List<Statement>
            {
                new Statement { MeetingID = "1234567890", EventID = new Guid(), Started = new DateTime(2023, 5, 15), Ended = new DateTime(2023, 5, 15), DurationSeconds = 54 },
                new Statement { MeetingID = "meetingB", EventID = new Guid(), Started = new DateTime(2023, 5, 15), Ended = new DateTime(2023, 5, 15), DurationSeconds = 54 },
            };
            var videoSyncs = new List<VideoSync>
            {
                new VideoSync { MeetingID = "1234567890", Timestamp = new DateTime(2023, 5, 14), VideoPosition = 34 },
                new VideoSync { MeetingID = "meetingA", Timestamp = null, VideoPosition = 43 }
            };
            var meeting = new Meeting
            {
                MeetingID = "meetingA",
                MeetingStarted = new DateTime(2023, 5, 15),
                MeetingEnded = new DateTime(2023, 5, 15),
                MeetingSequenceNumber = 1,
            };
            var statement = new Statement
            {
                MeetingID = "meetingA",
                EventID = new Guid(),
                Started = DateTime.UtcNow,
                Ended = DateTime.UtcNow,
                DurationSeconds = 45,
            };
            var webApiStatementsDTO = new WebApiStatementsDTO
            {
                MeetingId = "meetingA",
                VideoPosition = 54,
                VideoLink = "videoLink",
            };

            _statementsRepository.Setup(x => x.GetSatementsByName(personName, year, lang)).Returns(Task.FromResult(statements));
            _videoSyncRepository.Setup(x => x.GetVideoPositions("1234567890")).Returns(Task.FromResult(videoSyncs));
            _meetingsRepository.Setup(x => x.FetchMeetingById("1234567890")).Returns(Task.FromResult(meeting));
            _mapper.Setup(x => x.Map<WebApiStatementsDTO>(statement)).Returns(webApiStatementsDTO);

            var result = await _statementProvider.GetStatementsByPerson(personName, year, lang);

            _statementsRepository.Verify(x => x.GetSatementsByName(personName, year, lang), Times.Once);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async void GetStatementsByPerson_ReturnEmptyList()
        {
            var statements = new List<Statement>
            {
                new Statement { MeetingID = "meetingA", EventID = new Guid(), Started = new DateTime(2023, 5, 15) },
                new Statement { MeetingID = "meetingB", EventID = new Guid(), Started = new DateTime(2023, 5, 27) },
            };
            var videoSyncs = new List<VideoSync>
            {
                new VideoSync { MeetingID = "meetingA", Timestamp = null, VideoPosition = 0 },
                new VideoSync { MeetingID = "meetingA", Timestamp = null, VideoPosition = 43 }
            };

            _statementsRepository.Setup(x => x.GetSatementsByName(personName, year, lang)).Returns(Task.FromResult(statements));
            _videoSyncRepository.Setup(x => x.GetVideoPositions("meetingA")).Returns(Task.FromResult(videoSyncs));

            var result = await _statementProvider.GetStatementsByPerson(personName, year, lang);

            _statementsRepository.Verify(x => x.GetSatementsByName(personName, year, lang), Times.Once);
            Assert.Empty(result);
        }
    }
}