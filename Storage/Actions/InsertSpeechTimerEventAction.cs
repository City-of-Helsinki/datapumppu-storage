using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    public class InsertSpeechTimerEventAction : IEventAction
    {
        public List<EventType> EventTypes { get; } = new()
            { EventType.SpeechTimer };

        private readonly ISpeechTimerEventsRepository _speechTimerEventsRepository;

        public InsertSpeechTimerEventAction(ISpeechTimerEventsRepository speechTimerEventsRepository)
        {
            _speechTimerEventsRepository = speechTimerEventsRepository;
        }

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var speechTimerEventDto = eventBody.ToObjectFromJson<SpeechTimerEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SpeechTimerEventDTO, SpeechTimerEvent>()
                    .ForMember(dest => dest.MeetingID, opt => opt.MapFrom(x => speechTimerEventDto.MeetingID))
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var speechTimerEvent = mapper.Map<SpeechTimerEvent>(speechTimerEventDto);

            return _speechTimerEventsRepository.InsertSpeechTimerEvent(speechTimerEvent, connection, transaction);
        }
    }
}
