using System.Data;
using Moq;
using Storage;
using Storage.Actions;
using Storage.Controllers.Event.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using Xunit;

namespace StorageServiceUnitTests.Storage.Actions
{
    public class InsertEventActionMappingTest
    {
        [Fact]
        public async Task Execute_ShouldMapCorrectly()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var eventDto = new EventDTO
            {
                MeetingID = "meeting-1",
                CaseNumber = "123",
                ItemNumber = "456",
                Timestamp = DateTime.UtcNow,
                SequenceNumber = 1,
                EventType = EventType.Case,
                MeetingTitleFI = "Title FI",
                MeetingTitleSV = "Title SV"
            };
            var eventBody = BinaryData.FromObjectAsJson(eventDto);

            var eventsRepository = new Mock<IEventsRepository>();
            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();

            Event? capturedEvent = null;
            eventsRepository.Setup(x => x.InsertEvent(It.IsAny<Event>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
                .Callback<Event, IDbConnection, IDbTransaction>((e, c, t) => capturedEvent = e)
                .Returns(Task.CompletedTask);

            var action = new InsertEventAction(eventsRepository.Object);

            // Act
            await action.Execute(eventBody, eventId, connection.Object, transaction.Object);

            // Assert
            Assert.NotNull(capturedEvent);
            Assert.Equal(eventId, capturedEvent.EventID);
            Assert.Equal(eventDto.MeetingID, capturedEvent.MeetingID);
            Assert.Equal(eventDto.CaseNumber, capturedEvent.CaseNumber);
            Assert.Equal(eventDto.ItemNumber, capturedEvent.ItemNumber);
        }
    }
}
