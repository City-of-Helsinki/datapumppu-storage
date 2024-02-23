using System.Data;
using AutoMapper;
using Storage.Actions;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Actions
{
    public class InsertPauseInfoActionTest
    {
        [Fact]
        public async Task ExecuteShouldCallInsertPauseInfo()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            var pauseInfoDto = new PauseInfoEventDTO
            {
                MeetingID = "meetingId",
                Timestamp = DateTime.UtcNow,
                SequenceNumber = 1,
                Info = "info"
            };
            var eventBody = BinaryData.FromObjectAsJson(pauseInfoDto);

            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            var pauseInfo = new PauseInfo();
            var pauseInfoRepository = new Mock<IPauseInfoRepository>();
            var mapper = new Mock<IMapper>();
            var insertPauseInfoAction = new InsertPauseInfoAction(pauseInfoRepository.Object);

            pauseInfoRepository.Setup(x => x.InsertPauseInfo(pauseInfo, connection.Object, transaction.Object))
                .Returns(Task.CompletedTask);

            mapper.Setup(x => x.Map<PauseInfo>(pauseInfoDto)).Returns(pauseInfo);

            // Act
            await insertPauseInfoAction.Execute(eventBody, eventId, connection.Object, transaction.Object);

            // Assert
            pauseInfoRepository.Verify(x => x.InsertPauseInfo(
                It.IsAny<PauseInfo?>(), connection.Object, transaction.Object),
                Times.Once);
        }
    }
}