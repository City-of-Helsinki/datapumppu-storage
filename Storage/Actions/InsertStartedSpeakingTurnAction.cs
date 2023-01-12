using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    public class InsertStartedSpeakingTurnAction : IEventAction
    {
        public List<EventType> EventTypes { get; } = new()
            { EventType.SpeakingTurnStarted };

        private readonly ISpeakingTurnsRepository _speakingTurnsRepository;

        public InsertStartedSpeakingTurnAction(ISpeakingTurnsRepository speakingTurnsRepository)
        {
            _speakingTurnsRepository = speakingTurnsRepository;
        }

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var speakingTurnStartedDto = eventBody.ToObjectFromJson<SpeakingTurnStartedEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SpeakingTurnStartedEventDTO, StartedStatement>()
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var startedSpeakingTurn = mapper.Map<StartedStatement>(speakingTurnStartedDto);

            return _speakingTurnsRepository.InsertStartedSpeakingTurn(startedSpeakingTurn, connection, transaction);
        }
    }
}