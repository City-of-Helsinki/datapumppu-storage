using System.Data;
using AutoMapper;
using Storage.Actions;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Actions
{
    public class UpdateVotingStatusActionTest
    {
        [Fact]
        public async Task ExecuteShouldCallUpsertVotingStartedEvent()
        {
            var eventId = new Guid();
            var votingEventDto = new VotingEventDTO
            {
                VotingNumber = 3,
                MeetingID = "meetingId",
                EventType = (global::Storage.EventType)2,
            };

            var eventBody = BinaryData.FromObjectAsJson(votingEventDto);
            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            var votingEvent = new VotingEvent();
            var votingEventRepository = new Mock<IVotingsRepository>();
            var mapper = new Mock<IMapper>();
            var upsertVotingStartedAction = new UpdateVotingStatusAction(votingEventRepository.Object);

            votingEventRepository.Setup(x => x.UpsertVotingStartedEvent(votingEvent, connection.Object, transaction.Object))
            .Returns(Task.CompletedTask);

            mapper.Setup(x => x.Map<VotingEvent>(votingEventDto)).Returns(votingEvent);

            await upsertVotingStartedAction.Execute(eventBody, eventId, connection.Object, transaction.Object);

            votingEventRepository.Verify(x => x.UpsertVotingStartedEvent(
            It.IsAny<VotingEvent?>(), connection.Object, transaction.Object),
            Times.Once);
        }
    }
}