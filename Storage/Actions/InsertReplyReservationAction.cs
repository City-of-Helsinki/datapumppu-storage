using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    public class InsertReplyReservationAction : IEventAction
    {
        public List<EventType> EventTypes { get; } = new()
            { EventType.ReplyReservation };

        private readonly IStatementsRepository _statementsRepository;

        public InsertReplyReservationAction(IStatementsRepository statementsRepository)
        {
            _statementsRepository = statementsRepository;
        }

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var replyReservationEventDto = eventBody.ToObjectFromJson<ReplyReservationEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ReplyReservationEventDTO, ReplyReservation>()
                    .ForMember(dest => dest.MeetingID, opt => opt.MapFrom(x => replyReservationEventDto.MeetingID))
                    .ForMember(dest => dest.Active, opt => opt.Ignore())
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var replyReservation = mapper.Map<ReplyReservation>(replyReservationEventDto);

            return _statementsRepository.InsertReplyReservation(replyReservation, connection, transaction);
        }
    }
}
