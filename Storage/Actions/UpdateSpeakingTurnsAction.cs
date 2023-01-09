using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    public class UpdateSpeakingTurnsAction : IEventAction
    {
        public List<EventType> EventTypes { get; } = new()
            { EventType.SpeakingTurns };

        private readonly ISpeakingTurnsRepository _speakingTurnsRepository;

        public UpdateSpeakingTurnsAction(ISpeakingTurnsRepository speakingTurnsRepository)
        {
            _speakingTurnsRepository = speakingTurnsRepository;
        }

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var speakingTurnsEventDto = eventBody.ToObjectFromJson<SpeakingTurnsEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SpeakingTurnDTO, Statement>()
                    .ForMember(dest => dest.MeetingID, opt => opt.MapFrom(x => speakingTurnsEventDto.MeetingID))
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            var mapper = config.CreateMapper();
            var speakingTurns = speakingTurnsEventDto.SpeakingTurns.Select(speakingTurnDto => mapper.Map<Statement>(speakingTurnDto)).ToList();

            return _speakingTurnsRepository.UpsertSpeakingTurns(speakingTurns, connection, transaction);
        }
    }
}
