using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;

namespace Storage.Actions
{
    /// <summary>
    /// Defines the contract for upserting comprehensive meeting data including agendas, decisions, and attachments.
    /// </summary>
    public interface IUpsertMeetingAction
    {
        /// <summary>
        /// Executes the meeting upsert operation with all related data.
        /// </summary>
        /// <param name="meetingDTO">The complete meeting data including metadata, agenda items, decisions, and attachments.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Execute(MeetingDTO meetingDTO);
    }

    /// <summary>
    /// Handles bulk import or update of comprehensive meeting data from the API.
    /// Processes meeting metadata, agenda items with attachments and PDFs, decisions with attachments and PDFs,
    /// all within a single database transaction to ensure data consistency.
    /// This action is invoked directly from controllers, not through event processing.
    /// </summary>
    public class UpsertMeetingAction : IUpsertMeetingAction
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly IMeetingsRepository _meetingsRepository;
        private readonly IAgendaItemsRepository _agendaItemsRepository;
        private readonly IDecisionsRepository _decisionsRepository;
        private readonly ILogger<UpsertMeetingAction> _logger;

        /// <summary>
        /// Initializes a new instance of the UpsertMeetingAction with required dependencies.
        /// </summary>
        /// <param name="connectionFactory">Factory for creating database connections.</param>
        /// <param name="meetingsRepository">Repository for managing meeting data.</param>
        /// <param name="agendaItemsRepository">Repository for managing agenda item data and attachments.</param>
        /// <param name="decisionsRepository">Repository for managing decision data and attachments.</param>
        /// <param name="logger">Logger for recording operation status and errors.</param>
        public UpsertMeetingAction(IDatabaseConnectionFactory connectionFactory, IMeetingsRepository meetingsRepository,IAgendaItemsRepository agendaItemsRepository, IDecisionsRepository decisionsRepository, ILogger<UpsertMeetingAction> logger)
        {
            _connectionFactory = connectionFactory;
            _meetingsRepository = meetingsRepository;
            _agendaItemsRepository = agendaItemsRepository;
            _decisionsRepository = decisionsRepository;
            _logger = logger;
        }

        /// <summary>
        /// Executes the comprehensive meeting upsert operation.
        /// Maps the meeting DTO to repository models, processes all related entities (agendas, decisions, attachments),
        /// and executes the entire operation within a database transaction.
        /// </summary>
        /// <param name="meetingDTO">The complete meeting data transfer object.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Execute(MeetingDTO meetingDTO)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AgendaItemDTO, AgendaItem>()
                    .ForMember(dest => dest.MeetingID, opt => opt.MapFrom(_ => meetingDTO.MeetingID))
                    .ForMember(dest => dest.Timestamp, opt => opt.Ignore())
                    .ForMember(dest => dest.ItemTextFi, opt => opt.Ignore())
                    .ForMember(dest => dest.ItemNumber, opt => opt.Ignore())
                    .ForMember(dest => dest.EditorUserName, opt => opt.Ignore())
                    .ForMember(dest => dest.VideoPosition, opt => opt.Ignore());

                cfg.CreateMap<DecisionDTO, Decision>()
                    .ForMember(dest => dest.MeetingID, opt => opt.MapFrom(_ => meetingDTO.MeetingID));
                cfg.CreateMap<MeetingDTO, Meeting>()
                    .ForMember(dest => dest.MeetingTitleFI, opt => opt.Ignore())
                    .ForMember(dest => dest.MeetingTitleSV, opt => opt.Ignore())
                    .ForMember(dest => dest.MeetingStarted, opt => opt.Ignore())
                    .ForMember(dest => dest.MeetingStartedEventID, opt => opt.Ignore())
                    .ForMember(dest => dest.MeetingEnded, opt => opt.Ignore())
                    .ForMember(dest => dest.MeetingEndedEventID, opt => opt.Ignore());
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var agendas = meetingDTO.Agendas?.Select(agenda => mapper.Map<AgendaItem>(agenda)).ToList();

            var agendaItemAttachments = meetingDTO.Agendas?.SelectMany(agendaItem => agendaItem.Attachments,
                    (agendaItem, Attachments) => new { Attachments, agendaItem.AgendaPoint, meetingDTO.MeetingID })
                .Select(attachmentData => MapToAgendaItemAttachment(attachmentData.Attachments, attachmentData.AgendaPoint, attachmentData.MeetingID))
                .ToList();

            var agendaItemPdfs = meetingDTO.Agendas?.Where(agendaItem => agendaItem.Pdf != null)
                .Select(agendaItem => MapToAgendaItemAttachment(agendaItem.Pdf, agendaItem.AgendaPoint, meetingDTO.MeetingID)).ToList();

            //
            // not used at the moment
            //
            //var agendaItemDecisionHistoryPdfs = meetingDTO.Agendas?.Where(agendaItem => agendaItem.DecisionHistoryPdf != null)
            //    .Select(agendaItem => MapToAgendaItemAttachment(agendaItem.DecisionHistoryPdf, agendaItem.AgendaPoint, meetingDTO.MeetingID)).ToList();

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
                agendas, 
                agendaItemAttachments,
                agendaItemPdfs ?? new List<AgendaItemAttachment>(),
                decisions ?? new List<Decision>(), 
                decisionAttachments ?? new List<DecisionAttachment>(), 
                decisionPdfs ?? new List<DecisionAttachment>(), 
                decisionHistoryPdfs ?? new List<DecisionAttachment>());
        }

        /// <summary>
        /// Maps an AttachmentDTO to an AgendaItemAttachment model.
        /// Validates and normalizes the language code to ensure only valid values (fi, sv, en, null) are stored.
        /// </summary>
        /// <param name="attachmentDto">The attachment data transfer object.</param>
        /// <param name="agendaPoint">The agenda point number this attachment belongs to.</param>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <returns>An AgendaItemAttachment model instance.</returns>
        private AgendaItemAttachment MapToAgendaItemAttachment(AttachmentDTO attachmentDto, int agendaPoint, string meetingId)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AttachmentDTO, AgendaItemAttachment>()
                    .ForMember(dest => dest.Language, opt => opt.MapFrom(src => GetLanguage(src.Language)))
                    .ForMember(dest => dest.MeetingID, opt => opt.MapFrom(_ => meetingId))
                    .ForMember(dest => dest.AgendaPoint, opt => opt.MapFrom(_ => agendaPoint));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            return mapper.Map<AgendaItemAttachment>(attachmentDto);
        }

        /// <summary>
        /// Validates and normalizes language codes for attachments.
        /// Ensures only supported language codes (fi, sv, en) or null are returned.
        /// </summary>
        /// <param name="language">The language code to validate.</param>
        /// <returns>The validated language code, or null if the input is invalid.</returns>
        private string GetLanguage(string language)
        {
            var correctLanguages = new List<string?> { "fi", "sv", "en", null };
            if (correctLanguages.Contains(language)) 
            {
                return language;
            }

            return null;
        }

        /// <summary>
        /// Maps an AttachmentDTO to a DecisionAttachment model.
        /// </summary>
        /// <param name="attachmentDto">The attachment data transfer object.</param>
        /// <param name="decisionId">The native decision identifier this attachment belongs to.</param>
        /// <returns>A DecisionAttachment model instance.</returns>
        private DecisionAttachment MapToDecisionAttachment(AttachmentDTO attachmentDto, string decisionId)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AttachmentDTO, DecisionAttachment>()
                    .ForMember(dest => dest.DecisionId, opt => opt.MapFrom(x => decisionId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            return mapper.Map<DecisionAttachment>(attachmentDto);
        }

        /// <summary>
        /// Executes all meeting-related database operations within a single transaction.
        /// Upserts meeting metadata, agenda items with attachments and PDFs, and decisions with attachments and PDFs.
        /// Commits the transaction if all operations succeed, or rolls back on any failure.
        /// </summary>
        /// <param name="meeting">The meeting metadata.</param>
        /// <param name="agendas">The list of agenda items.</param>
        /// <param name="agendaItemAttachments">The list of agenda item attachments.</param>
        /// <param name="agendaItemPdfs">The list of agenda item PDF documents.</param>
        /// <param name="decisions">The list of decisions.</param>
        /// <param name="decisionAttachments">The list of decision attachments.</param>
        /// <param name="decisionPdfs">The list of decision PDF documents.</param>
        /// <param name="decisionHistoryPdfs">The list of decision history PDF documents.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task MakeTransaction(
            Meeting meeting,
            List<AgendaItem>? agendas,
            List<AgendaItemAttachment>? agendaItemAttachments,
            List<AgendaItemAttachment> agendaItemPdfs,
            List<Decision> decisions,
            List<DecisionAttachment> decisionAttachments,
            List<DecisionAttachment> decisionPdfs,
            List<DecisionAttachment> decisionHistoryPdfs)
        {
            using var connection = await _connectionFactory.CreateOpenConnection();

            var transaction = connection.BeginTransaction();
            try
            {
                await _meetingsRepository.UpsertMeeting(meeting, connection, transaction);

                if (agendas?.Count > 0)
                {
                    await _agendaItemsRepository.UpsertAgendaItems(agendas, connection, transaction);
                }

                var validAttachments = agendaItemAttachments?.Where(attachment => attachment.AttachmentNumber != null).ToList();
                if (validAttachments?.Count > 0)
                {
                    await _agendaItemsRepository.UpsertAgendaItemAttachments(validAttachments, connection, transaction);
                }
                await _agendaItemsRepository.UpsertAgendaItemPdfs(agendaItemPdfs, connection, transaction);
                await _agendaItemsRepository.UpsertAgendaItemDecisionHistoryPdfs(agendaItemPdfs, connection, transaction);
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
