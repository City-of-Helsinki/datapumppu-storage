using System.Data;
using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace Storage.Actions
{
    /// <summary>
    /// Handles RollCallStarted and RollCallEnded events that track attendance verification during meetings.
    /// Manages roll call sessions to record which participants are present.
    /// </summary>
    public class UpsertRollCallAction : IEventAction
    {
        /// <summary>
        /// Gets the event types handled by this action. Processes RollCallStarted and RollCallEnded events.
        /// </summary>
        public List<EventType> EventTypes { get; } = new()
            { EventType.RollCallStarted, EventType.RollCallEnded };

        private readonly IRollCallRepository _rollCallRepository;

        /// <summary>
        /// Initializes a new instance of the UpsertRollCallAction with the required repository.
        /// </summary>
        /// <param name="rollCallRepository">Repository for managing roll call data.</param>
        public UpsertRollCallAction(IRollCallRepository rollCallRepository)
        {
            _rollCallRepository = rollCallRepository;
        }

        /// <summary>
        /// Executes the action to upsert roll call information.
        /// For RollCallStarted events, records the start of attendance verification.
        /// For RollCallEnded events, records the completion of roll call.
        /// </summary>
        /// <param name="eventBody">The binary event payload containing roll call event data.</param>
        /// <param name="eventId">The unique identifier for this event.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The database transaction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var rollCallEventDto = eventBody.ToObjectFromJson<RollCallEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<RollCallEventDTO, RollCall>()
                    .ForMember(dest => dest.RollCallStarted, opt =>
                    {
                        opt.PreCondition(src => src.EventType == EventType.RollCallStarted);
                        opt.MapFrom(src => src.Timestamp);
                    })
                    .ForMember(dest => dest.RollCallStartedEventID, opt =>
                    {
                        opt.PreCondition(src => src.EventType == EventType.RollCallStarted);
                        opt.MapFrom(x => eventId);
                    })
                    .ForMember(dest => dest.RollCallEnded, opt =>
                    {
                        opt.PreCondition(src => src.EventType == EventType.RollCallEnded);
                        opt.MapFrom(src => src.Timestamp);
                    })
                    .ForMember(dest => dest.RollCallEndedEventID, opt =>
                    {
                        opt.PreCondition(src => src.EventType == EventType.RollCallEnded);
                        opt.MapFrom(x => eventId);
                    });
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var rollCall = mapper.Map<RollCall>(rollCallEventDto);

            if (rollCallEventDto.EventType == EventType.RollCallStarted)
            {
                return _rollCallRepository.UpsertRollCallStarted(rollCall, connection, transaction);
            }
            return _rollCallRepository.UpsertRollCallEnded(rollCall, connection, transaction);
        }
    }
}
