
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
        new() { MeetingID = "meetingA", EventID = new Guid(), Started = DateTime.UtcNow },
        new() { MeetingID = "meetingB", EventID = new Guid() },
      };
      var videoSyncs = new List<VideoSync>
      {
        new() { MeetingID = "meetingA", Timestamp = DateTime.UtcNow, VideoPosition = 0 },
        new() { MeetingID = "meetingA", Timestamp = null, VideoPosition = 43 }
      };

      _statementsRepository.Setup(x => x.GetSatementsByName(personName, year, lang)).Returns(Task.FromResult(statements));
      _videoSyncRepository.Setup(x => x.GetVideoPositions("meetingA")).Returns(Task.FromResult(videoSyncs));

      var result = await _statementProvider.GetStatementsByPerson(personName, year, lang);

      _statementsRepository.Verify(x => x.GetSatementsByName(personName, year, lang), Times.Once);
      Assert.NotEmpty(result);
    }

    [Fact]
    public async void GetStatementsByPerson_ReturnPossibleNullData()
    {
      var statements = new List<Statement>
      {
        new() { MeetingID = "meetingA", EventID = new Guid(), Started = new DateTime(2023, 5, 15) },
        new() { MeetingID = "meetingB", EventID = new Guid(), Started = new DateTime(2023, 5, 27) },
      };
      var videoSyncs = new List<VideoSync>
      {
        new() { MeetingID = "meetingA", Timestamp = null, VideoPosition = 0 },
        new() { MeetingID = "meetingA", Timestamp = null, VideoPosition = 43 }
      };

      _statementsRepository.Setup(x => x.GetSatementsByName(personName, year, lang)).Returns(Task.FromResult(statements));
      _videoSyncRepository.Setup(x => x.GetVideoPositions("meetingA")).Returns(Task.FromResult(videoSyncs));

      var result = await _statementProvider.GetStatementsByPerson(personName, year, lang);

      _statementsRepository.Verify(x => x.GetSatementsByName(personName, year, lang), Times.Once);
      Assert.NotEmpty(result);
    }
  }
}