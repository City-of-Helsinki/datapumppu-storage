using System.Data;
using AutoMapper;
using Storage.Actions;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Actions
{
    public class InsertStartedStatementActionTest
    {
        [Fact]
        public async Task ExecuteShouldCallInsertStartedStatement()
        {
            var eventId = new Guid();

            var statementStartedDto = new StatementStartedEventDTO
            {
                MeetingID = "meetingId",
                Timestamp = DateTime.UtcNow,
                SequenceNumber = 1,
                CaseNumber = "caseNumber",
                ItemNumber = "itemNumber"
            };

            var eventBody = BinaryData.FromObjectAsJson(statementStartedDto);
            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            var startedStatement = new StartedStatement();
            var startedStatementRepository = new Mock<IStatementsRepository>();
            var mapper = new Mock<IMapper>();
            var insertStartedStatementAction = new InsertStartedStatementAction(startedStatementRepository.Object);

            startedStatementRepository.Setup(x => x.InsertStartedStatement(startedStatement, connection.Object, transaction.Object))
                .Returns(Task.CompletedTask);

            mapper.Setup(x => x.Map<StartedStatement>(statementStartedDto)).Returns(startedStatement);

            await insertStartedStatementAction.Execute(eventBody, eventId, connection.Object, transaction.Object);

            startedStatementRepository.Verify(x => x.InsertStartedStatement(
            It.IsAny<StartedStatement?>(), connection.Object, transaction.Object),
            Times.Once);
        }
    }
}