using AutoMapper;
using Confluent.Kafka;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Events.Providers;
using Storage.Repositories;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;

namespace Storage.Actions
{
    public interface IUpsertAgendaPointAction
    {
        Task<bool> Execute(AgendaPointEditDTO editDto);
    }

    public class UpsertAgendaPointAction : IUpsertAgendaPointAction
    {
        private readonly IConfiguration _configuration;
        private readonly IKafkaClientFactory _kafkaClientFactory;
        private readonly IAgendaItemsRepository _agendaItemsRepository;
        private readonly IMeetingsRepository _meetingsRepository;
        private readonly ILogger<UpsertAgendaPointAction> _logger;

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

        public async Task<bool> Execute(AgendaPointEditDTO agendaDTO)
        {
            var meeting = await _meetingsRepository.FetchMeetingById(agendaDTO.MeetingId);
            if (meeting == null || meeting.MeetingStarted < DateTime.Now.AddDays(-7))
            {
                return false;
            }

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
