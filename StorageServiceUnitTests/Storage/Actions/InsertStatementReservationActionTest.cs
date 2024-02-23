using System.Data;
using AutoMapper;
using Storage.Actions;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Actions
{
    public class InsertStatementReservationActionTest
    {
        [Fact]
        public async Task ExecuteShouldCallInsertStatementReservation()
        {
            var eventId = new Guid();

            var statementReservationDTO = new StatementReservationEventDTO
            {
                Person = "Person A",
                SeatID = "seatA",
                Ordinal = 1,
            };

            var eventBody = BinaryData.FromObjectAsJson(statementReservationDTO);
            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            var statementReservation = new StatementReservation();
            var statementReservationRepository = new Mock<IStatementsRepository>();
            var mapper = new Mock<IMapper>();
            var insertStatementReservationAction = new InsertStatementReservationAction(statementReservationRepository.Object);

            statementReservationRepository.Setup(x => x.InsertStatementReservation(statementReservation, connection.Object, transaction.Object))
                .Returns(Task.CompletedTask);

            mapper.Setup(x => x.Map<StatementReservation>(statementReservationDTO)).Returns(statementReservation);

            await insertStatementReservationAction.Execute(eventBody, eventId, connection.Object, transaction.Object);

            statementReservationRepository.Verify(x => x.InsertStatementReservation(
            It.IsAny<StatementReservation?>(), connection.Object, transaction.Object),
            Times.Once);
        }
    }
}