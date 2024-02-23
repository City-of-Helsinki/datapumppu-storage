using System.Data;
using AutoMapper;
using Storage.Actions;
using Storage.Controllers.Event.DTOs;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Actions
{
    public class UpdateMeetingStatusActionTest
    {
        [Fact]
        public async Task ExecuteShouldCallUpsertMeetingStartTime()
        {
            var eventId = new Guid();
            var meetingStatusDto = new SimpleEventDTO
            {
            MeetingID = "meetingId",
            EventType = 0,
            SequenceNumber = 1,
            CaseNumber = "caseA"
            };

            var eventBody = BinaryData.FromObjectAsJson(meetingStatusDto);
            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            var meeting = new Meeting();
            var meetingRepository = new Mock<IMeetingsRepository>();
            var mapper = new Mock<IMapper>();
            var updateMeetingAction = new UpdateMeetingStatusAction(meetingRepository.Object);

            meetingRepository.Setup(x => x.UpsertMeetingStartTime(meeting, connection.Object, transaction.Object))
            .Returns(Task.CompletedTask);

            mapper.Setup(x => x.Map<Meeting>(meetingStatusDto)).Returns(meeting);

            await updateMeetingAction.Execute(eventBody, eventId, connection.Object, transaction.Object);

            meetingRepository.Verify(x => x.UpsertMeetingStartTime(
            It.IsAny<Meeting>(), connection.Object, transaction.Object),
            Times.Once);
        }
    }
}