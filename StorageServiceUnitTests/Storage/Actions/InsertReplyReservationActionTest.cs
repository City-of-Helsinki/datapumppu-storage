using System.Data;
using AutoMapper;
using Storage.Actions;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Actions
{
    public class InsertReplyReservationActionTest
    {
        [Fact]
        public async Task ExecuteShouldCallInsertReplyReservation()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            var replyReservationEventDto = new ReplyReservationEventDTO
            {
                MeetingID = "meetingId",
                Timestamp = DateTime.UtcNow,
                SequenceNumber = 1,
            };
            var eventBody  = BinaryData.FromObjectAsJson(replyReservationEventDto);

            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            var replyReservation = new ReplyReservation();
            var replyReservationRepository = new Mock<IStatementsRepository>();
            var mapper = new Mock<IMapper>();
            var insertReplyReservationAction = new InsertReplyReservationAction(replyReservationRepository.Object);

            replyReservationRepository.Setup(x => x.InsertReplyReservation(replyReservation, connection.Object, transaction.Object))
                .Returns(Task.CompletedTask);

            mapper.Setup(x => x.Map<ReplyReservation>(replyReservationEventDto)).Returns(replyReservation);

            // Act
            await insertReplyReservationAction.Execute(eventBody, eventId, connection.Object, transaction.Object);

            // Assert
            replyReservationRepository.Verify(x => x.InsertReplyReservation(
                It.IsAny<ReplyReservation?>(), connection.Object, transaction.Object),
                Times.Once);
        }
    }    
}