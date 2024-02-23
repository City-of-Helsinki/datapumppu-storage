using System.Data;
using AutoMapper;
using Storage;
using Storage.Actions;
using Storage.Controllers.Event.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Actions
{
    public class InsertEventActionTest
    {
        [Fact]
        public async Task ExecuteShouldCallInsertEvent()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            var eventDto = new EventDTO
            {
                MeetingID = "meetingId",
                EventType = EventType.Case,
                Timestamp = DateTime.UtcNow,
                SequenceNumber = 1,
                CaseNumber = "caseNumber",
                ItemNumber = "itemNumber"
            };
            var eventBody = BinaryData.FromObjectAsJson(eventDto);

            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            var meetingEvent = new Event();
            var eventsRepository = new Mock<IEventsRepository>();
            var mapper = new Mock<IMapper>();
            var insertEventAction = new InsertEventAction(eventsRepository.Object);

            eventsRepository.Setup(x => x.InsertEvent(meetingEvent, connection.Object, transaction.Object))
                .Returns(Task.CompletedTask);

            mapper.Setup(x => x.Map<Event>(eventDto)).Returns(meetingEvent);

            // Act
            await insertEventAction.Execute(eventBody, eventId, connection.Object, transaction.Object);

            // Assert
            eventsRepository.Verify(x => x.InsertEvent(
                It.IsAny<Event?>(), connection.Object, transaction.Object),
                Times.Once);
        }

    }
}