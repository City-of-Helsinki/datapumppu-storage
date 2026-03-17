using AutoMapper;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Events.Providers;
using Storage.Repositories;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;

namespace Storage.Actions
{
    /// <summary>
    /// Defines the contract for upserting agenda point HTML content and triggering Kafka notifications.
    /// </summary>
    public interface IUpsertAgendaPointAction
    {
        /// <summary>
        /// Executes the agenda point upsert operation and publishes a Kafka event.
        /// </summary>
        /// <param name="editDto">The agenda point edit data containing meeting ID, agenda point number, HTML content, language, and editor information.</param>
        /// <returns>True if the operation succeeded; false if the meeting is too old or doesn't exist.</returns>
        Task<bool> Execute(AgendaPointEditDTO editDto);
    }

    /// <summary>
    /// Handles manual editing of agenda point HTML content from the API.
    /// Validates meeting age (must be within 7 days of start), updates agenda item HTML, and publishes a Kafka event to notify consumers.
    /// This action is invoked directly from controllers, not through event processing.
    /// </summary>
    public class UpsertAgendaPointAction : IUpsertAgendaPointAction
    {
        private readonly IConfiguration _configuration;
        private readonly IKafkaClientFactory _kafkaClientFactory;
        private readonly IAgendaItemsRepository _agendaItemsRepository;
        private readonly IMeetingsRepository _meetingsRepository;
        private readonly ILogger<UpsertAgendaPointAction> _logger;

        /// <summary>
        /// Initializes a new instance of the UpsertAgendaPointAction with required dependencies.
        /// </summary>
        /// <param name="connectionFactory">Factory for creating database connections (unused in current implementation).</param>
        /// <param name="agendaItemsRepository">Repository for managing agenda item data.</param>
        /// <param name="meetingsRepository">Repository for fetching meeting information.</param>
        /// <param name="kafkaClientFactory">Factory for creating Kafka producers.</param>
        /// <param name="configuration">Application configuration for Kafka topics and other settings.</param>
        /// <param name="logger">Logger for recording operation details and warnings.</param>
        public UpsertAgendaPointAction(
            IDatabaseConnectionFactory connectionFactory,
            IAgendaItemsRepository agendaItemsRepository,
            IMeetingsRepository meetingsRepository,
            IKafkaClientFactory kafkaClientFactory,
            IConfiguration configuration,
            ILogger<UpsertAgendaPointAction> logger)
        {
            _agendaItemsRepository = agendaItemsRepository;
            _meetingsRepository = meetingsRepository;
            _kafkaClientFactory = kafkaClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Executes the agenda point HTML update operation.
        /// Validates that the meeting exists and started within the last 7 days, updates the agenda item HTML,
        /// and publishes a Kafka message to notify other services of the change.
        /// </summary>
        /// <param name="agendaDTO">The agenda point edit data.</param>
        /// <returns>True if successful; false if the meeting is too old, doesn't exist, or started more than 7 days ago.</returns>
        public async Task<bool> Execute(AgendaPointEditDTO agendaDTO)
        {
            var meeting = await _meetingsRepository.FetchMeetingById(agendaDTO.MeetingId);
            if (meeting == null || meeting.MeetingStarted < DateTime.Now.AddDays(-7))
            {
                _logger.LogWarning("meeting {0} is too old {1} for editing", agendaDTO.MeetingId, meeting?.MeetingStarted.ToString());
                return false;
            }

            _logger.LogInformation("meeting {0} was started {1}", agendaDTO.MeetingId, meeting?.MeetingStarted.ToString());

            var agendaItem = new AgendaItem
            {
                MeetingID = agendaDTO.MeetingId,
                AgendaPoint = agendaDTO.AgendaPoint,
                Html = agendaDTO.Html,
                Language = agendaDTO.Language,
                EditorUserName = agendaDTO.EditorUserName,
            };

            await _agendaItemsRepository.UpsertAgendaItemHtml(agendaItem);

            var producer = _kafkaClientFactory.CreateProducer();

            var producerTopic = _configuration["KAFKA_PRODUCER_TOPIC"];

            var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(new { MeetingID = agendaDTO.MeetingId, CaseNumber = agendaDTO.AgendaPoint.ToString(), IsLiveEvent = false });
            await producer.ProduceAsync(producerTopic, new Message<Null, string> { Value = jsonBody });

            return true;
        }
    }
}
