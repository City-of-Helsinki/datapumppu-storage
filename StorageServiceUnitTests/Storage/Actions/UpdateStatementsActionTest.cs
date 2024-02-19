using System.Data;
using AutoMapper;
using Storage.Actions;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Actions
{
  public class UpdateStatementsActionTest
  {
    [Fact]
    public async Task ExecuteShouldCallUpsertStatements()
    {
      var eventId = new Guid();

      List<StatementDTO> statementDtos = new()
      {
        new StatementDTO { Person = "personA", Duration = 432 },
        new StatementDTO { Person = "personB", Duration = 35 },
        new StatementDTO { Person = "personC", Duration = 235 },
      };

      var statementsDto = new StatementsEventDTO
      {
        Statements = statementDtos
      };

      List<Statement> statements = new()
      {
        new Statement { MeetingID = "meetingId", EventID = new Guid() },
        new Statement { MeetingID = "meetingId1", EventID = new Guid() },
        new Statement { MeetingID = "meetingId2", EventID = new Guid() },
      };

      var eventBody = BinaryData.FromObjectAsJson(statementsDto);
      var connection = new Mock<IDbConnection>();
      var transaction = new Mock<IDbTransaction>();
      var statement = new Statement();
      var statementRepository = new Mock<IStatementsRepository>();
      var mapper = new Mock<IMapper>();
      var updateStatementAction = new UpdateStatementsAction(statementRepository.Object);

      statementRepository.Setup(x => x.UpsertStatements(statements, connection.Object, transaction.Object))
        .Returns(Task.CompletedTask);

      mapper.Setup(x => x.Map<Statement>(statementsDto)).Returns(statement);

      await updateStatementAction.Execute(eventBody, eventId, connection.Object, transaction.Object);

      statementRepository.Verify(x => x.UpsertStatements(
        It.IsAny<List<Statement>?>(), connection.Object, transaction.Object),
        Times.Once);
    }
  }
}