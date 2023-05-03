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
        Task Execute(AgendaPointEditDTO editDto);
    }

    public class UpsertAgendaPointAction : IUpsertAgendaPointAction
    {
        private readonly IConfiguration _configuration;
        private readonly IKafkaClientFactory _kafkaClientFactory;
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly IAgendaItemsRepository _agendaItemsRepository;
        private readonly ILogger<UpsertAgendaPointAction> _logger;

        public UpsertAgendaPointAction(
            IDatabaseConnectionFactory connectionFactory,
            IAgendaItemsRepository agendaItemsRepository,
            IKafkaClientFactory kafkaClientFactory,
            IConfiguration configuration,
            ILogger<UpsertAgendaPointAction> logger)
        {
            _connectionFactory = connectionFactory;
            _agendaItemsRepository = agendaItemsRepository;
            _kafkaClientFactory = kafkaClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task Execute(AgendaPointEditDTO agendaDTO)
        {
            var agendaItem = new AgendaItem
            {
                MeetingID = agendaDTO.MeetingId,
                AgendaPoint = agendaDTO.AgendaPoint,
                Html = agendaDTO.Html,
                Language = agendaDTO.Language
            };

            await _agendaItemsRepository.UpsertAgendaItemHtml(agendaItem);

            var producer = _kafkaClientFactory.CreateProducer();

            var producerTopic = _configuration["KAFKA_PRODUCER_TOPIC"];

            var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(new { MeetingID = agendaDTO.MeetingId, CaseNumber = agendaDTO.AgendaPoint.ToString() });
            await producer.ProduceAsync(producerTopic, new Message<Null, string> { Value = jsonBody });
        }
    }
}
