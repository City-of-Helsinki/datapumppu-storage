using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    /// <summary>
    /// Handles ReplyReservation events when a participant reserves the right to reply during a meeting.
    /// Manages the queue of reply reservations for statements.
    /// </summary>
    public class InsertReplyReservationAction : IEventAction
    {
        /// <summary>
        /// Gets the event types handled by this action. Only processes ReplyReservation events.
        /// </summary>
        public List<EventType> EventTypes { get; } = new()
            { EventType.ReplyReservation };

        private readonly IStatementsRepository _statementsRepository;

        /// <summary>
        /// Initializes a new instance of the InsertReplyReservationAction with the required repository.
        /// </summary>
        /// <param name="statementsRepository">Repository for persisting reply reservation records.</param>
        public InsertReplyReservationAction(IStatementsRepository statementsRepository)
        {
            _statementsRepository = statementsRepository;
        }

        /// <summary>
        /// Executes the action to insert a reply reservation record.
        /// Stores a participant's request to reply to a statement.
        /// </summary>
        /// <param name="eventBody">The binary event payload containing reply reservation data.</param>
        /// <param name="eventId">The unique identifier for this event.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The database transaction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var replyReservationEventDto = eventBody.ToObjectFromJson<ReplyReservationEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ReplyReservationEventDTO, ReplyReservation>()
                    .ForMember(dest => dest.MeetingID, opt => opt.MapFrom(x => replyReservationEventDto.MeetingID))
                    .ForMember(dest => dest.Active, opt => opt.Ignore())
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var replyReservation = mapper.Map<ReplyReservation>(replyReservationEventDto);

            return _statementsRepository.InsertReplyReservation(replyReservation, connection, transaction);
        }
    }
}
