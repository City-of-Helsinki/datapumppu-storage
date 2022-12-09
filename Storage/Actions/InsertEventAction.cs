using AutoMapper;
using Storage.Controllers.Event.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    public class InsertEventAction : IEventAction
    {
        private readonly IEventsRepository _eventsRepository;

        public InsertEventAction(IEventsRepository eventsRepository)
        {
            _eventsRepository = eventsRepository;
        }

        public List<EventType> EventTypes { get; } = Enum.GetValues(typeof(EventType)).Cast<EventType>().ToList();

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var eventDto = eventBody.ToObjectFromJson<EventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<EventDTO, Event>()
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            var mapper = config.CreateMapper();
            var meetingEvent = mapper.Map<Event>(eventDto);

            return _eventsRepository.InsertEvent(meetingEvent, connection, transaction);
        }
    }
}
