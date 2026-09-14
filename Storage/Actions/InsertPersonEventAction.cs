using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    /// <summary>
    /// Handles person arrival and departure events for meeting participants.
    /// Tracks when participants enter or leave the meeting room.
    /// </summary>
    public class InsertPersonEventAction : IEventAction
    {
        /// <summary>
        /// Gets the event types handled by this action. Processes PersonArrived and PersonLeft events.
        /// </summary>
        public List<EventType> EventTypes { get; } = new()
            { EventType.PersonArrived, EventType.PersonLeft };

        private readonly IPersonEventsRepository _personEventsRepository;

        /// <summary>
        /// Initializes a new instance of the InsertPersonEventAction with the required repository.
        /// </summary>
        /// <param name="personEventsRepository">Repository for persisting person event records.</param>
        public InsertPersonEventAction(IPersonEventsRepository personEventsRepository)
        {
            _personEventsRepository = personEventsRepository;
        }

        /// <summary>
        /// Executes the action to insert a person event record.
        /// Records participant arrival or departure from the meeting.
        /// </summary>
        /// <param name="eventBody">The binary event payload containing person event data.</param>
        /// <param name="eventId">The unique identifier for this event.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The database transaction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var personEventDto = eventBody.ToObjectFromJson<PersonEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PersonEventDTO, PersonEvent>()
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var personEvent = mapper.Map<PersonEvent>(personEventDto);

            return _personEventsRepository.InsertPersonEvent(personEvent, connection, transaction);
        }
    }
}