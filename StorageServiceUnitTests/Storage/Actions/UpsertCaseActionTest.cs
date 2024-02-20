using System.Data;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Storage.Actions;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;

namespace StorageServiceUnitTests.Storage.Actions
{
    public class UpsertCaseActionTest
  {
    [Fact]
    public async Task ExecuteShouldCallMakeTransaction()
    {
      var meetingDto = new MeetingDTO
      {
        MeetingID = "meetingId",
      };

      var agendaDto = new AgendaItemDTO();
      var decisionDto = new DecisionDTO();

      var mapper = new Mock<IMapper>();
      var eventBody = BinaryData.FromObjectAsJson(meetingDto);
      var connection = new Mock<IDbConnection>();
      var transaction = new Mock<IDbTransaction>();
      var upsertMeeting = new Mock<IUpsertMeetingAction>();
      var meeting = new Meeting();
      var agendaItem = new AgendaItem();
      var decision = new Decision();

      var meetingsRepository = new Mock<IMeetingsRepository>();
      var agendasRepository = new Mock<IAgendaItemsRepository>();
      var decisionsRepository = new Mock<IDecisionsRepository>();
      var connectionFactory = new Mock<IDatabaseConnectionFactory>();
      var logger = new Mock<ILogger<UpsertMeetingAction>>();
      var upsertMeetingAction = new UpsertMeetingAction(connectionFactory.Object, meetingsRepository.Object,
        agendasRepository.Object, decisionsRepository.Object,
        logger.Object);

      connectionFactory.Setup(x => x.CreateOpenConnection()).Returns(() => {return Task.FromResult(connection.Object);});
      connection.Setup(x => x.BeginTransaction()).Returns(transaction.Object);
      upsertMeeting.Setup(x => x.Execute(meetingDto)).Returns(Task.CompletedTask);

      mapper.Setup(x => x.Map<Meeting>(meetingDto)).Returns(meeting);
      mapper.Setup(x => x.Map<AgendaItem>(agendaDto)).Returns(agendaItem);
      mapper.Setup(x => x.Map<Decision>(decisionDto)).Returns(decision);

      await upsertMeetingAction.Execute(meetingDto);

      upsertMeeting.Verify(x => x.Execute(
        It.IsAny<MeetingDTO?>()),
        Times.Once);
    }
  }
}