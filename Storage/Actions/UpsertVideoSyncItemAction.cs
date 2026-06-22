using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Transactions;

namespace Storage.Actions
{
    /// <summary>
    /// Defines the contract for upserting video synchronization items that link meeting content to video timestamps.
    /// </summary>
    public interface IUpsertVideoSyncItemAction
    {
        /// <summary>
        /// Executes the video sync item upsert operation.
        /// </summary>
        /// <param name="videoSyncDto">The video synchronization data containing meeting ID, case number, and video position.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Execute(VideoSyncDTO videoSyncDto);
    }

    /// <summary>
    /// Handles manual creation or update of video synchronization items from the API.
    /// Maps meeting agenda items to specific timestamps in recorded meeting videos for playback synchronization.
    /// This action is invoked directly from controllers, not through event processing.
    /// </summary>
    public class UpsertVideoSyncItemAction: IUpsertVideoSyncItemAction
    {
        private readonly IVideoSyncRepository _videoSyncRepository;

        /// <summary>
        /// Initializes a new instance of the UpsertVideoSyncItemAction with the required repository.
        /// </summary>
        /// <param name="videoSyncRepository">Repository for managing video synchronization data.</param>
        public UpsertVideoSyncItemAction(IVideoSyncRepository videoSyncRepository)
        {
            _videoSyncRepository = videoSyncRepository;
        }

        /// <summary>
        /// Executes the video sync item upsert operation.
        /// Creates or updates a video synchronization record linking agenda items to video timestamps.
        /// </summary>
        /// <param name="videoSyncDto">The video synchronization data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Execute(VideoSyncDTO videoSyncDto)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VideoSyncDTO, VideoSync>();
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var videoSyncItem = mapper.Map<VideoSync>(videoSyncDto);

            return _videoSyncRepository.UpsertVideoSyncItem(videoSyncItem);
        }
    }
}
