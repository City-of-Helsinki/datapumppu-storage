using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Transactions;

namespace Storage.Actions
{
    public interface IUpsertVideoSyncItemAction
    {
        Task Execute(VideoSyncDTO videoSyncDto);
    }

    public class UpsertVideoSyncItemAction: IUpsertVideoSyncItemAction
    {
        private readonly IVideoSyncRepository _videoSyncRepository;

        public UpsertVideoSyncItemAction(IVideoSyncRepository videoSyncRepository)
        {
            _videoSyncRepository = videoSyncRepository;
        }

        public Task Execute(VideoSyncDTO videoSyncDto)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VideoSyncDTO, VideoSync>();
            });
            var mapper = config.CreateMapper();
            var videoSyncItem = mapper.Map<VideoSync>(videoSyncDto);

            return _videoSyncRepository.UpsertVideoSyncItem(videoSyncItem);
        }
    }
}
