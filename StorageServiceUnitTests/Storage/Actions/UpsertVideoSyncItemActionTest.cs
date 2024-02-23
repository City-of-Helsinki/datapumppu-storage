using AutoMapper;
using Storage.Actions;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Actions
{
    public class UpsertVideoSyncItemActionTest
    {
        [Fact]
        public async Task ExecuteShouldCallUpsertVideoSyncItem()
        {
            var eventId = new Guid();
            var videoSyncDto = new VideoSyncDTO
            {
                MeetingID = "meetingId",
                Timestamp = DateTime.UtcNow,
                VideoPosition = 54
            };

            var eventBody = BinaryData.FromObjectAsJson(videoSyncDto);
            var videoSync = new VideoSync();
            var videoSyncRepository = new Mock<IVideoSyncRepository>();
            var mapper = new Mock<IMapper>();
            var upsertVideoSyncItemAction = new UpsertVideoSyncItemAction(videoSyncRepository.Object);

            videoSyncRepository.Setup(x => x.UpsertVideoSyncItem(videoSync)).Returns(Task.FromResult<int>);

            mapper.Setup(x => x.Map<VideoSync>(videoSyncDto)).Returns(videoSync);

            await upsertVideoSyncItemAction.Execute(videoSyncDto);

            videoSyncRepository.Verify(x => x.UpsertVideoSyncItem(
            It.IsAny<VideoSync?>()),
            Times.Once);
        }
    }
}