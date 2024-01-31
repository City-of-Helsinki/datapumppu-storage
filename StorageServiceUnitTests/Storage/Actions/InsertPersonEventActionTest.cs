using System.Data;
using AutoMapper;
using Storage.Actions;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Actions
{
    public class InsertPersonEventActionTest
    {
        [Fact]
        public async Task ExecuteShouldCallInsertPersonEvent()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            var personEventDto = new PersonEventDTO
            {
                MeetingID = "meetingId",
                Timestamp = DateTime.UtcNow,
                SequenceNumber = 1,
                Person = "person",
                SeatID = "1",
            };
            var eventBody  = BinaryData.FromObjectAsJson(personEventDto);

            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            var personEvent = new PersonEvent();
            var personEventRepository = new Mock<IPersonEventsRepository>();
            var mapper = new Mock<IMapper>();
            var insertPersonEventAction = new InsertPersonEventAction(personEventRepository.Object);

            personEventRepository.Setup(x => x.InsertPersonEvent(personEvent, connection.Object, transaction.Object))
                .Returns(Task.CompletedTask);

            mapper.Setup(x => x.Map<PersonEvent>(personEventDto)).Returns(personEvent);

            // Act
            await insertPersonEventAction.Execute(eventBody, eventId, connection.Object, transaction.Object);

            // Assert
            personEventRepository.Verify(x => x.InsertPersonEvent(
                It.IsAny<PersonEvent?>(), connection.Object, transaction.Object),
                Times.Once);
        }
    }

    internal interface IPersonEventRepository
    {
    }
}