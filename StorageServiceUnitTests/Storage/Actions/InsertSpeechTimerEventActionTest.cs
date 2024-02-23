using System.Data;
using AutoMapper;
using Storage.Actions;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Actions
{
    public class InsertSpeechTimerEventActionTest
    {
        [Fact]
        public async Task ExecuteShouldCallInsertSpeechTimerEvent()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            var speechTimerEventDto = new SpeechTimerEventDTO
            {
                MeetingID = "meetingId",
                Timestamp = DateTime.UtcNow,
                SequenceNumber = 1,
            };
            var eventBody = BinaryData.FromObjectAsJson(speechTimerEventDto);

            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            var speechTimerEvent = new SpeechTimerEvent();
            var speechTimerEventRepository = new Mock<ISpeechTimerEventsRepository>();
            var mapper = new Mock<IMapper>();
            var insertSpeechTimerEventAction = new InsertSpeechTimerEventAction(speechTimerEventRepository.Object);

            speechTimerEventRepository.Setup(x => x.InsertSpeechTimerEvent(speechTimerEvent, connection.Object, transaction.Object))
                .Returns(Task.CompletedTask);

            mapper.Setup(x => x.Map<SpeechTimerEvent>(speechTimerEventDto)).Returns(speechTimerEvent);

            // Act
            await insertSpeechTimerEventAction.Execute(eventBody, eventId, connection.Object, transaction.Object);

            // Assert
            speechTimerEventRepository.Verify(x => x.InsertSpeechTimerEvent(
                It.IsAny<SpeechTimerEvent?>(), connection.Object, transaction.Object),
                Times.Once);
        }
    }
}