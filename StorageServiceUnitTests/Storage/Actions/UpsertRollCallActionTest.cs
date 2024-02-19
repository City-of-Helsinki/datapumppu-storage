using System.Data;
using AutoMapper;
using Storage.Actions;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Actions
{
    public class UpsertRollCallActionTest
  {
    [Fact]
    public async Task ExecuteShouldCallUpsertRollCallStarted()
    {
      var eventId = new Guid();
      var rollCallEventDto = new RollCallEventDTO
      {
        MeetingID = "meetingId",
        EventType = (global::Storage.EventType)7
      };

      var eventBody = BinaryData.FromObjectAsJson(rollCallEventDto);
      var connection = new Mock<IDbConnection>();
      var transaction = new Mock<IDbTransaction>();
      var rollCallRepository = new Mock<IRollCallRepository>();
      var mapper = new Mock<IMapper>();
      var rollCallEvent = new RollCall();
      var upsertRollCall = new UpsertRollCallAction(rollCallRepository.Object);

      rollCallRepository.Setup(x => x.UpsertRollCallStarted(rollCallEvent, connection.Object, transaction.Object))
        .Returns(Task.CompletedTask);

      mapper.Setup(x => x.Map<RollCall>(rollCallEventDto)).Returns(rollCallEvent);

      await upsertRollCall.Execute(eventBody, eventId, connection.Object, transaction.Object);

      rollCallRepository.Verify(x => x.UpsertRollCallStarted(
        It.IsAny<RollCall?>(), connection.Object, transaction.Object),
        Times.Once);
    }
  }
}