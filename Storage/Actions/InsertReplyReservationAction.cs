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

        private readonly IReplyReservationsRepository _replyReservationsRepository;

        public InsertReplyReservationAction(IReplyReservationsRepository replyReservationsRepository)
        {
            _replyReservationsRepository = replyReservationsRepository;
        }

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var replyReservationEventDto = eventBody.ToObjectFromJson<ReplyReservationEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ReplyReservationEventDTO, ReplyReservation>()
                    .ForMember(dest => dest.MeetingID, opt => opt.MapFrom(x => replyReservationEventDto.MeetingID))
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var replyReservation = mapper.Map<ReplyReservation>(replyReservationEventDto);

            return _replyReservationsRepository.InsertReplyReservation(replyReservation, connection, transaction);
        }
    }
}
