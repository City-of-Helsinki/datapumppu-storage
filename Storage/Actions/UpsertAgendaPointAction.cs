using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
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
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly IAgendaItemsRepository _agendaItemsRepository;
        private readonly ILogger<UpsertAgendaPointAction> _logger;

        public UpsertAgendaPointAction(
            IDatabaseConnectionFactory connectionFactory,
            IAgendaItemsRepository agendaItemsRepository,
            ILogger<UpsertAgendaPointAction> logger)
        {
            _connectionFactory = connectionFactory;
            _agendaItemsRepository = agendaItemsRepository;
            _logger = logger;
        }

        public Task Execute(AgendaPointEditDTO agendaDTO)
        {
            var agendaItem = new AgendaItem
            {
                MeetingID = agendaDTO.MeetingId,
                AgendaPoint = agendaDTO.AgendaPoint,
                Html = agendaDTO.Html,
                Language = agendaDTO.Language
            };

            return _agendaItemsRepository.UpsertAgendaItemHtml(agendaItem);
        }
    }
}
