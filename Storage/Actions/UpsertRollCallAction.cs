using System.Data;
using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace Storage.Actions
{
    public class UpsertRollCallAction : IEventAction
    {
        public List<EventType> EventTypes { get; } = new()
            { EventType.RollCallStarted, EventType.RollCallEnded };

        private readonly IRollCallRepository _rollCallRepository;

        public UpsertRollCallAction(IRollCallRepository rollCallRepository)
        {
            _rollCallRepository = rollCallRepository;
        }

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var rollCallEventDto = eventBody.ToObjectFromJson<RollCallEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<RollCallEventDTO, RollCall>()
                    .ForMember(dest => dest.RollCallStarted, opt =>
                    {
                        opt.PreCondition(src => src.EventType == EventType.RollCallStarted);
                        opt.MapFrom(src => src.Timestamp);
                    })
                    .ForMember(dest => dest.RollCallStartedEventID, opt =>
                    {
                        opt.PreCondition(src => src.EventType == EventType.RollCallStarted);
                        opt.MapFrom(x => eventId);
                    })
                    .ForMember(dest => dest.RollCallEnded, opt =>
                    {
                        opt.PreCondition(src => src.EventType == EventType.RollCallEnded);
                        opt.MapFrom(src => src.Timestamp);
                    })
                    .ForMember(dest => dest.RollCallEndedEventID, opt =>
                    {
                        opt.PreCondition(src => src.EventType == EventType.RollCallEnded);
                        opt.MapFrom(x => eventId);
                    });
            });
            var mapper = config.CreateMapper();
            var rollCall = mapper.Map<RollCall>(rollCallEventDto);

            if (rollCallEventDto.EventType == EventType.RollCallStarted)
            {
                return _rollCallRepository.UpsertRollCallStarted(rollCall, connection, transaction);
            }
            return _rollCallRepository.UpsertRollCallEnded(rollCall, connection, transaction);
        }
    }
}
