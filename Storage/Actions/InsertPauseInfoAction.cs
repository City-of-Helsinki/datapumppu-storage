using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    /// <summary>
    /// Handles PauseInfo events that indicate meeting pauses or breaks.
    /// Stores information about when meetings are paused and resumed.
    /// </summary>
    public class InsertPauseInfoAction : IEventAction
    {
        /// <summary>
        /// Gets the event types handled by this action. Only processes PauseInfo events.
        /// </summary>
        public List<EventType> EventTypes { get; } = new()
            { EventType.PauseInfo };

        private readonly IPauseInfoRepository _pauseInfoRepository;

        /// <summary>
        /// Initializes a new instance of the InsertPauseInfoAction with the required repository.
        /// </summary>
        /// <param name="pauseInfoRepository">Repository for persisting pause information records.</param>
        public InsertPauseInfoAction(IPauseInfoRepository pauseInfoRepository)
        {
            _pauseInfoRepository = pauseInfoRepository;
        }

        /// <summary>
        /// Executes the action to insert a pause info record.
        /// Deserializes the pause event data and stores it in the database.
        /// </summary>
        /// <param name="eventBody">The binary event payload containing pause information.</param>
        /// <param name="eventId">The unique identifier for this event.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The database transaction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var breakNoticeEventDto = eventBody.ToObjectFromJson<PauseInfoEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PauseInfoEventDTO, PauseInfo>()
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var breakNotice = mapper.Map<PauseInfo>(breakNoticeEventDto);

            return _pauseInfoRepository.InsertPauseInfo(breakNotice, connection, transaction);
        }
    }
}