using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    public class InsertPersonEventAction : IEventAction
    {
        public List<EventType> EventTypes { get; } = new()
            { EventType.PersonArrived, EventType.PersonLeft };

        private readonly IPersonEventsRepository _personEventsRepository;

        public InsertPersonEventAction(IPersonEventsRepository personEventsRepository)
        {
            _personEventsRepository = personEventsRepository;
        }

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var personEventDto = eventBody.ToObjectFromJson<PersonEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PersonEventDTO, PersonEvent>()
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            var mapper = config.CreateMapper();
            var personEvent = mapper.Map<PersonEvent>(personEventDto);

            return _personEventsRepository.InsertPersonEvent(personEvent, connection, transaction);
        }
    }
}