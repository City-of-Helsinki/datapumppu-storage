using Dapper;
using Newtonsoft.Json;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;
using System.Data;

namespace Storage.Repositories
{
    public interface IAgendaItemsRepository
    {
        Task<List<AgendaItem>> FetchAgendasByMeetingId(string id, string language);

        Task UpsertAgendaItems(List<AgendaItem> agendasItems, IDbConnection connection, IDbTransaction transaction);

        Task UpsertAgendaItemAttachments(List<AgendaItemAttachment> agendaAttachments, IDbConnection connection, IDbTransaction transaction);

        Task UpsertAgendaItemPdfs(List<AgendaItemAttachment> agendaItemPdfs, IDbConnection connection, IDbTransaction transaction);

        Task UpsertAgendaItemDecisionHistoryPdfs(List<AgendaItemAttachment> agendaItemHistoryPdfs, IDbConnection connection, IDbTransaction transaction);
    }

    public class AgendaItemsRepository : IAgendaItemsRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly ILogger<AgendaItemsRepository> _logger;

        public AgendaItemsRepository(IDatabaseConnectionFactory connectionFactory, ILogger<AgendaItemsRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<List<AgendaItem>> FetchAgendasByMeetingId(string id, string language)
        {
            using var connection = await _connectionFactory.CreateOpenConnection();
            var sqlQuery = @"
                SELECT meeting_id, agenda_point, section, title, case_id_label, html_content Html, html_decision_history DecisionHistoryHtml, language
                FROM agenda_items
                WHERE meeting_id = @id AND language = @language
            ";
            var result = (await connection.QueryAsync<AgendaItem>(sqlQuery, new { @id, @language })).ToList();

            return result;
        }

        public Task UpsertAgendaItems(List<AgendaItem> agendaItems, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Upserting agenda items");
            var sqlQuery = @"INSERT INTO agenda_items (meeting_id, agenda_point, section, title, case_id_label, html_content, html_decision_history, language) values(
                @meetingId, 
                @agendaPoint,
                @section,
                @title,
                @caseIdLabel,
                @html,
                @decisionHistoryHtml,
                @language
            ) ";
            sqlQuery += @"ON CONFLICT (meeting_id, agenda_point, language) DO UPDATE SET 
                section = @section,
                case_id_label = @caseIdLabel,
                html_content = @html,
                html_decision_history = @decisionHistoryHtml,
                title = @title
                WHERE agenda_items.meeting_id = @meetingId and agenda_items.agenda_point = @agendaPoint and agenda_items.language = @language
            ;";

            return connection.ExecuteAsync(sqlQuery, agendaItems.Select(item => new
            {
                meetingId = item.MeetingID,
                agendaPoint = item.AgendaPoint,
                section = item.Section,
                title = item.Title,
                caseIdLabel = item.CaseIDLabel,
                html = item.Html,
                decisionHistoryHtml = item.DecisionHistoryHtml,
                language = item.Language
            }), transaction);
        }

        public Task UpsertAgendaItemAttachments(List<AgendaItemAttachment> attachments,
    IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Upserting agendaitem attachments");
            var sqlQuery = @"INSERT INTO agenda_item_attachments (meeting_id, agenda_point, native_id, title, attachment_number,
                publicity_class, security_reasons, type, file_uri, language, personal_data, issued) values(
                @meetingId,
                @agendaPoint,
                @nativeId,
                @title,
                @attachmentNumber,
                @publicityClass,
                @securityReasons,
                @type,
                @fileUri,
                @language,
                @personalData,
                @issued
            ) ";
            sqlQuery += @"ON CONFLICT (meeting_id, agenda_point, attachment_number) DO UPDATE SET 
                title = @title,
                publicity_class = @publicityClass,
                security_reasons = @securityReasons,
                native_id = @nativeId,
                type = @type,
                file_uri = @fileUri,
                language = @language,
                personal_data = @personalData,
                issued = @issued
                WHERE agenda_item_attachments.meeting_id = @meetingId 
                AND agenda_item_attachments.agenda_point = @agendaPoint 
                AND agenda_item_attachments.attachment_number = @attachmentNumber
            ;";

            return connection.ExecuteAsync(sqlQuery, attachments.Select(item => {
                Console.WriteLine("working on " + JsonConvert.SerializeObject(item));
                return new
                {
                    meetingId = item.MeetingID,
                    agendaPoint = item.AgendaPoint,
                    nativeId = item.NativeId,
                    title = item.Title,
                    attachmentNumber = item.AttachmentNumber,
                    publicityClass = item.PublicityClass,
                    securityReasons = item.SecurityReasons,
                    type = item.Type,
                    fileUri = item.FileURI,
                    language = item.Language,
                    personalData = item.PersonalData,
                    issued = item.Issued
                };
            }), transaction);
        }
        public Task UpsertAgendaItemPdfs(List<AgendaItemAttachment> decisionPdfs,
    IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Upserting agendaitem pdfs");
            var sqlQuery = @"INSERT INTO agenda_item_pdfs (meeting_id, agenda_point, native_id, title, attachment_number, publicity_class,
                security_reasons, type, file_uri, language, personal_data, issued) values(
                @meetingId,
                @agendaPoint,
                @nativeId,
                @title,
                @attachmentNumber,
                @publicityClass,
                @securityReasons,
                @type,
                @fileUri,
                @language,
                @personalData,
                @issued
            ) ";
            sqlQuery += @"ON CONFLICT (meeting_id, agenda_point) DO UPDATE SET 
                native_id = @nativeId,
                title = @title,
                attachment_number = @attachmentNumber,
                publicity_class = @publicityClass,
                security_reasons = @securityReasons,
                type = @type,
                file_uri = @fileUri,
                language = @language,
                personal_data = @personalData,
                issued = @issued
                WHERE agenda_item_pdfs.meeting_id = @meetingId
                AND agenda_item_pdfs.agenda_point = @agendaPoint
            ;";

            return connection.ExecuteAsync(sqlQuery, decisionPdfs.Select(item => new
            {
                meetingId = item.MeetingID,
                agendaPoint = item.AgendaPoint,
                nativeId = item.NativeId,
                title = item.Title,
                attachmentNumber = item.AttachmentNumber,
                publicityClass = item.PublicityClass,
                securityReasons = item.SecurityReasons,
                type = item.Type,
                fileUri = item.FileURI,
                language = item.Language,
                personalData = item.PersonalData,
                issued = item.Issued
            }), transaction);
        }

        public Task UpsertAgendaItemDecisionHistoryPdfs(List<AgendaItemAttachment> decisionHistoryPdfs,
    IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Upserting agendaitem DecisionHistoryPdfs");
            var sqlQuery = @"INSERT INTO agenda_item_decision_history_pdfs (meeting_id, agenda_point, native_id, title, attachment_number, publicity_class, 
                security_reasons, type, file_uri, language, personal_data, issued) values(
                @meetingId,
                @agendaPoint,
                @nativeId,
                @title,
                @attachmentNumber,
                @publicityClass,
                @securityReasons,
                @type,
                @fileUri,
                @language,
                @personalData,
                @issued
            ) ";
            sqlQuery += @"ON CONFLICT (meeting_id, agenda_point) DO UPDATE SET 
                native_id = @nativeId,
                title = @title,
                attachment_number = @attachmentNumber,
                publicity_class = @publicityClass,
                security_reasons = @securityReasons,
                type = @type,
                file_uri = @fileUri,
                language = @language,
                personal_data = @personalData,
                issued = @issued
                WHERE agenda_item_decision_history_pdfs.meeting_id = @meetingId
                AND agenda_item_decision_history_pdfs.agenda_point = @agendaPoint
            ;";

            return connection.ExecuteAsync(sqlQuery, decisionHistoryPdfs.Select(item => new
            {
                meetingId = item.MeetingID,
                agendaPoint = item.AgendaPoint,
                nativeId = item.NativeId,
                title = item.Title,
                attachmentNumber = item.AttachmentNumber,
                publicityClass = item.PublicityClass,
                securityReasons = item.SecurityReasons,
                type = item.Type,
                fileUri = item.FileURI,
                language = item.Language,
                personalData = item.PersonalData,
                issued = item.Issued
            }), transaction);
        }

    }
}

