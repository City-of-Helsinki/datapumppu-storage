using System.Data;
using AutoMapper;
using Storage.Actions;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Actions
{
    public class UpsertMeetingActionTest
    {
        [Fact]
        public async Task ExecuteShouldCallUpsertCase()
        {
            var eventId = new Guid();
            var caseEventDto = new CaseEventDTO
            {
            MeetingID = "meetingId",
            };

            var eventBody = BinaryData.FromObjectAsJson(caseEventDto);
            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            var caseRepository = new Mock<ICaseRepository>();
            var mapper = new Mock<IMapper>();
            var caseEvent = new Case();
            var upsertCaseAction = new UpsertCaseAction(caseRepository.Object);

            caseRepository.Setup(x => x.UpsertCase(caseEvent, connection.Object, transaction.Object))
            .Returns(Task.CompletedTask);

            mapper.Setup(x => x.Map<Case>(caseEventDto)).Returns(caseEvent);

            await upsertCaseAction.Execute(eventBody, eventId, connection.Object, transaction.Object);

            caseRepository.Verify(x => x.UpsertCase(
            It.IsAny<Case?>(), connection.Object, transaction.Object),
            Times.Once);
        }
    }
}