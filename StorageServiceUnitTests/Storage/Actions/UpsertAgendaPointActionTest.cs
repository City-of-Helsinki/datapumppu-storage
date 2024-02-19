using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Storage.Actions;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Events.Providers;
using Storage.Repositories;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;

namespace StorageServiceUnitTests.Storage.Actions
{
    public class UpsertAgendaPointActionTest
  {
    [Fact]
    public async Task ExecuteShouldCallUpsertAgendaItemHtml()
    {
      var agendaDto = new AgendaPointEditDTO
      {
        MeetingId = "meetingId",
        AgendaPoint = 1,
        Html = "<p>Hello from test</p>",
        Language = "Finnish",
        EditorUserName = "editorA"
      };

      var agendaItem = new AgendaItem
      {
        MeetingID = agendaDto.MeetingId,
        AgendaPoint = agendaDto.AgendaPoint,
        Html = agendaDto.Html,
        Language = agendaDto.Language,
        EditorUserName = agendaDto.EditorUserName
      };

      var agendaItemsRepository = new Mock<IAgendaItemsRepository>();
      var meetingsRepository = new Mock<IMeetingsRepository>();
      var connectionFactory = new Mock<IDatabaseConnectionFactory>();
      var kafkaClientFactory = new Mock<IKafkaClientFactory>();
      var configuration = new Mock<IConfiguration>();
      var logger = new Mock<ILogger<UpsertAgendaPointAction>>();
      var mapper = new Mock<IMapper>();
      var agendaItem2 = new AgendaItem();

      var upsertAgendaPointAction = new UpsertAgendaPointAction(
        connectionFactory.Object, agendaItemsRepository.Object,
        meetingsRepository.Object, kafkaClientFactory.Object,
        configuration.Object, logger.Object);

      agendaItemsRepository.Setup(x => x.UpsertAgendaItemHtml(agendaItem)).Returns(Task.CompletedTask);
      meetingsRepository.Setup(x => x.FetchMeetingById(It.IsAny<string>())).Returns(Task.FromResult(new Meeting { MeetingStarted = new DateTime() }));

      await upsertAgendaPointAction.Execute(agendaDto);

      agendaItemsRepository.Verify(x => x.UpsertAgendaItemHtml(
        It.IsAny<AgendaItem?>()),
        Times.Once);
    }
  }
}