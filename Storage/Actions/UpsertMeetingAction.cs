using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;

namespace Storage.Actions
{
    public interface IUpsertMeetingAction
    {
        Task Execute(MeetingDTO meetingDTO);
    }

    public class UpsertMeetingAction : IUpsertMeetingAction
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly IMeetingsRepository _meetingsRepository;
        private readonly IAgendaItemsRepository _agendaItemsRepository;
        private readonly IDecisionsRepository _decisionsRepository;
        private readonly ILogger<UpsertMeetingAction> _logger;

        public UpsertMeetingAction(IDatabaseConnectionFactory connectionFactory, IMeetingsRepository meetingsRepository,IAgendaItemsRepository agendaItemsRepository, IDecisionsRepository decisionsRepository, ILogger<UpsertMeetingAction> logger)
        {
            _connectionFactory = connectionFactory;
            _meetingsRepository = meetingsRepository;
            _agendaItemsRepository = agendaItemsRepository;
            _decisionsRepository = decisionsRepository;
            _logger = logger;
        }

        public async Task Execute(MeetingDTO meetingDTO)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AgendaItemDTO, AgendaItem>()
                    .ForMember(dest => dest.MeetingID, opt => opt.MapFrom(_ => meetingDTO.MeetingID));
                cfg.CreateMap<DecisionDTO, Decision>()
                    .ForMember(dest => dest.MeetingID, opt => opt.MapFrom(_ => meetingDTO.MeetingID));
                cfg.CreateMap<MeetingDTO, Meeting>();
            });
            var mapper = config.CreateMapper();

            var agendas = meetingDTO.Agendas?.Select(agenda => mapper.Map<AgendaItem>(agenda)).ToList();

            var decisions = meetingDTO.Decisions?.Select(decision => mapper.Map<Decision>(decision)).ToList();

            var decisionPdfs = meetingDTO.Decisions?.Where(decision => decision.Pdf != null)
                    .Select(decision => MapToDecisionAttachment(decision.Pdf, decision.NativeId)).ToList();

            var decisionHistoryPdfs = meetingDTO.Decisions?.Where(decision => decision.DecisionHistoryPdf != null)
                    .Select(decision => MapToDecisionAttachment(decision.DecisionHistoryPdf, decision.NativeId)).ToList();

            var decisionAttachments = meetingDTO.Decisions?.SelectMany(decision => decision.Attachments,
                        (decision, Attachments) => new { Attachments, decision.NativeId })
                    .Select(decisionAttachmentData => MapToDecisionAttachment(decisionAttachmentData.Attachments, decisionAttachmentData.NativeId))
                    .ToList();

            var meeting = mapper.Map<Meeting>(meetingDTO);
            
            await MakeTransaction(
                meeting, 
                agendas ?? new List<AgendaItem>(), 
                decisions ?? new List<Decision>(), 
                decisionAttachments ?? new List<DecisionAttachment>(), 
                decisionPdfs ?? new List<DecisionAttachment>(), 
                decisionHistoryPdfs ?? new List<DecisionAttachment>());
        }

        private DecisionAttachment MapToDecisionAttachment(DecisionAttachmentDTO attachmentDto, string decisionId)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<DecisionAttachmentDTO, DecisionAttachment>()
                    .ForMember(dest => dest.DecisionId, opt => opt.MapFrom(x => decisionId));
            });
            var mapper = config.CreateMapper();

            return mapper.Map<DecisionAttachment>(attachmentDto);
        }

        private async Task MakeTransaction(Meeting meeting, List<AgendaItem> agendas, List<Decision> decisions, List<DecisionAttachment> decisionAttachments, List<DecisionAttachment> decisionPdfs, List<DecisionAttachment> decisionHistoryPdfs)
        {
            using var connection = await _connectionFactory.CreateOpenConnection();

            var transaction = connection.BeginTransaction();
            try
            {
                await _meetingsRepository.UpsertMeeting(meeting, connection, transaction);
                await _agendaItemsRepository.UpsertAgendaItems(agendas, connection, transaction);
                await _decisionsRepository.UpsertDecisions(decisions, connection, transaction);
                await _decisionsRepository.UpsertDecisionAttachments(decisionAttachments, connection, transaction);
                await _decisionsRepository.UpsertDecisionPdfs(decisionPdfs, connection, transaction);
                await _decisionsRepository.UpsertDecisionHistoryPdfs(decisionHistoryPdfs, connection, transaction);
                transaction.Commit();
                _logger.LogInformation("Meeting data successfully stored!");
            }
            catch (Exception ex)
            {
                _logger.LogError("Transaction failed: " + ex.Message);
                transaction.Rollback();
            }
        } 
    }
}
