using AutoMapper;
using Storage.Controllers.Event.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    /// <summary>
    /// Inserts a generic event record into the events table for all event types.
    /// This action serves as a universal event logger, storing basic event metadata for auditing and tracking purposes.
    /// </summary>
    public class InsertEventAction : IEventAction
    {
        private readonly IEventsRepository _eventsRepository;

        /// <summary>
        /// Initializes a new instance of the InsertEventAction with the required repository.
        /// </summary>
        /// <param name="eventsRepository">Repository for persisting event records.</param>
        public InsertEventAction(IEventsRepository eventsRepository)
        {
            _eventsRepository = eventsRepository;
        }

        /// <summary>
        /// Gets all event types. This action handles every event type by inserting a base event record.
        /// </summary>
        public List<EventType> EventTypes { get; } = Enum.GetValues(typeof(EventType)).Cast<EventType>().ToList();

        /// <summary>
        /// Executes the action to insert a generic event record.
        /// Maps the event DTO to an Event model and persists it to the database.
        /// </summary>
        /// <param name="eventBody">The binary event payload containing the event data.</param>
        /// <param name="eventId">The unique identifier for this event.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The database transaction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var eventDto = eventBody.ToObjectFromJson<EventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<EventDTO, Event>()
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var meetingEvent = mapper.Map<Event>(eventDto);

            return _eventsRepository.InsertEvent(meetingEvent, connection, transaction);
        }
    }
}
