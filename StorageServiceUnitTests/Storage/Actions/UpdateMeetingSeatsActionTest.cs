using System.Data;
using AutoMapper;
using Storage.Actions;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Actions
{
    public class UpdateMeetingSeatsActionTest
    {
        [Fact]
        public async Task ExecuteShouldCallInsertMeetingSeatUpdate()
        {
            var eventId = new Guid();
            List<MeetingSeatDTO> attendees = new()
            {
                new MeetingSeatDTO { SeatID = "seatA" },
                new MeetingSeatDTO { SeatID = "seatB" }
            };
            List<MeetingSeat> meetingSeats = new()
            {
            new MeetingSeat { SeatID = "seatA", Person = "personA" },
            new MeetingSeat { SeatID = "seatB", Person = "personB" },
            };

            var attendeesEventDto = new AttendeesEventDTO
            {
            MeetingSeats = attendees,
            };

            var eventBody = BinaryData.FromObjectAsJson(attendeesEventDto);
            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            var meetingSeatUpdate = new MeetingSeatUpdate();
            var meetingSeatUpdateRepository = new Mock<IMeetingSeatsRepository>();
            var mapper = new Mock<IMapper>();
            var insertMeetingSeatUpdateAction = new UpdateMeetingSeatsAction(meetingSeatUpdateRepository.Object);

            meetingSeatUpdateRepository.Setup(x => x.InsertMeetingSeatUpdate(meetingSeatUpdate, meetingSeats, connection.Object, transaction.Object))
            .Returns(Task.CompletedTask);

            mapper.Setup(x => x.Map<MeetingSeatUpdate>(attendeesEventDto)).Returns(meetingSeatUpdate);

            await insertMeetingSeatUpdateAction.Execute(eventBody, eventId, connection.Object, transaction.Object);

            meetingSeatUpdateRepository.Verify(x => x.InsertMeetingSeatUpdate(
            It.IsAny<MeetingSeatUpdate?>(), It.IsAny<List<MeetingSeat>?>(), connection.Object, transaction.Object),
            Times.Once);
        }
    }
}