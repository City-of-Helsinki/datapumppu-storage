using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    public class InsertSpeakingTurnReservationAction : IEventAction
    {
        public List<EventType> EventTypes { get; } = new()
            { EventType.SpeakingTurnReservation };

        private readonly ISpeakingTurnsRepository _speakingTurnsRepository;

        public InsertSpeakingTurnReservationAction(ISpeakingTurnsRepository speakingTurnsRepository)
        {
            _speakingTurnsRepository = speakingTurnsRepository;
        }

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var speakingTurnReservationDto = eventBody.ToObjectFromJson<SpeakingTurnReservationEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SpeakingTurnReservationEventDTO, SpeakingTurnReservation>()
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            var mapper = config.CreateMapper();
            var speakingTurnReservation = mapper.Map<SpeakingTurnReservation>(speakingTurnReservationDto);

            return _speakingTurnsRepository.InsertSpeakingTurnReservation(speakingTurnReservation, connection, transaction);
        }
    }
}