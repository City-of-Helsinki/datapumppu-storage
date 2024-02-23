using System.Data;
using AutoMapper;
using Storage.Actions;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Actions
{
    public class InsertPropositionsEventActionTest
    {
        [Fact]
        public async Task ExecuteShouldCallInsertPropositions()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            var propositionsEventDto = new PropositionsEventDTO
            {
                MeetingID = "meetingId",
                Timestamp = DateTime.UtcNow,
                SequenceNumber = 1,
                Propositions = new List<PropositionDTO>()
                {
                    new PropositionDTO
                    {

                    }
                }
            };
            var eventBody = BinaryData.FromObjectAsJson(propositionsEventDto);

            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            var propositions = new List<Proposition>();
            var propositionsRepository = new Mock<IPropositionsRepository>();
            var mapper = new Mock<IMapper>();
            var insertPropositionsEventAction = new InsertPropositionsEventAction(propositionsRepository.Object);

            propositionsRepository.Setup(x => x.InsertPropositions(propositions, connection.Object, transaction.Object))
                .Returns(Task.CompletedTask);

            mapper.Setup(x => x.Map<List<Proposition>>(propositionsEventDto.Propositions)).Returns(propositions);

            // Act
            await insertPropositionsEventAction.Execute(eventBody, eventId, connection.Object, transaction.Object);

            // Assert
            propositionsRepository.Verify(x => x.InsertPropositions(
                It.IsAny<List<Proposition>>(), connection.Object, transaction.Object),
                Times.Once);
        }
    }
}