using Dapper;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;
using System.Data;

namespace Storage.Repositories
{
    /// <summary>
    /// Provides data access methods for agenda item management including items, attachments, and PDFs.
    /// </summary>
    public interface IAgendaItemsRepository
    {
        /// <summary>
        /// Retrieves sub-items for a specific agenda point in a meeting.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="agendaPoint">The agenda point number.</param>
        /// <returns>A list of agenda sub-items.</returns>
        Task<List<AgendaSubItem>> FetchAgendaSubItems(string meetingId, int agendaPoint);

        /// <summary>
        /// Retrieves all agenda items for a meeting in the specified language.
        /// </summary>
        /// <param name="id">The meeting identifier.</param>
        /// <param name="language">The language code (e.g., 'fi' for Finnish, 'sv' for Swedish).</param>
        /// <returns>A list of agenda items with associated case information.</returns>
        Task<List<AgendaItem>> FetchAgendasByMeetingId(string id, string language);

        /// <summary>
        /// Retrieves all agenda item attachments for a meeting in the specified language.
        /// </summary>
        /// <param name="id">The meeting identifier.</param>
        /// <param name="language">The language code for filtering attachments.</param>
        /// <returns>A list of agenda item attachments.</returns>
        Task<List<AgendaItemAttachment>> FetchAgendaAttachmentsByMeetingId(string id, string language);

        /// <summary>
        /// Retrieves all agenda items for meetings in a specific year.
        /// </summary>
        /// <param name="year">The year to query (e.g., 2024).</param>
        /// <returns>A list of agenda items from all meetings in the year.</returns>
        Task<List<AgendaItem>> FetchAgendasByYear(int year);

        /// <summary>
        /// Updates the HTML content for a specific agenda item.
        /// </summary>
        /// <param name="agendaItem">The agenda item with updated HTML content.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpsertAgendaItemHtml(AgendaItem agendaItem);

        /// <summary>
        /// Inserts or updates multiple agenda items within a transaction.
        /// </summary>
        /// <param name="agendasItems">The list of agenda items to upsert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpsertAgendaItems(List<AgendaItem> agendasItems, IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Inserts or updates agenda item attachments within a transaction.
        /// </summary>
        /// <param name="agendaAttachments">The list of attachments to upsert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpsertAgendaItemAttachments(List<AgendaItemAttachment> agendaAttachments, IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Inserts or updates agenda item PDF documents within a transaction.
        /// </summary>
        /// <param name="agendaItemPdfs">The list of PDF attachments to upsert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpsertAgendaItemPdfs(List<AgendaItemAttachment> agendaItemPdfs, IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Inserts or updates agenda item decision history PDF documents within a transaction.
        /// </summary>
        /// <param name="agendaItemHistoryPdfs">The list of decision history PDFs to upsert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpsertAgendaItemDecisionHistoryPdfs(List<AgendaItemAttachment> agendaItemHistoryPdfs, IDbConnection connection, IDbTransaction transaction);
    }

    /// <summary>
    /// Implements agenda item data access operations using Dapper for PostgreSQL queries.
    /// Manages meeting agenda items, attachments, and associated documents with bilingual support.
    /// </summary>
    public class AgendaItemsRepository : IAgendaItemsRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        private readonly ILogger<AgendaItemsRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the AgendaItemsRepository class.
        /// </summary>
        /// <param name="connectionFactory">Factory for creating database connections.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        public AgendaItemsRepository(IDatabaseConnectionFactory connectionFactory, ILogger<AgendaItemsRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves sub-items for a specific agenda point, typically representing items with non-zero item numbers.
        /// </summary>
        /// <param name="meetingId">The meeting identifier.</param>
        /// <param name="agendaPoint">The agenda point number to query sub-items for.</param>
        /// <returns>A list of agenda sub-items with text and numbering information.</returns>
        public async Task<List<AgendaSubItem>> FetchAgendaSubItems(string meetingId, int agendaPoint)
        {
            using var connection = await _connectionFactory.CreateOpenConnection();
            var sqlQuery = @"
                SELECT
                    item_text_fi,
                    item_number,
                    case_number::int8 as AgendaPoint
                FROM
                    cases
                WHERE
                    meeting_id = @meetingId
                    AND
                    case_number::int8 = @agendaPoint
                    AND
                    item_number != '0'
            ";
            var result = (await connection.QueryAsync<AgendaSubItem>(sqlQuery, new { meetingId, agendaPoint })).ToList();

            return result;
        }

        /// <summary>
        /// Retrieves all agenda items for meetings in a specific year, using the meeting ID pattern.
        /// </summary>
        /// <param name="year">The year to query (e.g., 2024). Only Finnish language items are returned.</param>
        /// <returns>A list of agenda items with meeting ID, agenda point, and title.</returns>
        public async Task<List<AgendaItem>> FetchAgendasByYear(int year)
        {
            var meetingId = $"02900{year}%";
            
            var sqlQuery = @"
                SELECT
                    agenda_items.meeting_id,
                    agenda_point,
                    title
                FROM
                    agenda_items
                WHERE
                    agenda_items.meeting_id like @meetingId
                    AND
                    language = 'fi'
            ";

            using var connection = await _connectionFactory.CreateOpenConnection();
            return (await connection.QueryAsync<AgendaItem>(sqlQuery, new { meetingId })).ToList();
        }

        /// <summary>
        /// Retrieves all agenda items for a specific meeting with timestamps from associated events.
        /// Joins with cases and meeting_events tables to provide complete agenda information.
        /// </summary>
        /// <param name="id">The meeting identifier.</param>
        /// <param name="language">The language code for filtering items ('fi' or 'sv').</param>
        /// <returns>A list of agenda items with HTML content, case information, and timestamps.</returns>
        public async Task<List<AgendaItem>> FetchAgendasByMeetingId(string id, string language)
        {
            using var connection = await _connectionFactory.CreateOpenConnection();
            var sqlQuery = @"
                SELECT
                    agenda_items.meeting_id,
                    agenda_point,
                    section,
                    title,
                    case_id_label,
                    html_content Html,
                    html_decision_history DecisionHistoryHtml,
                    language,
                    meeting_events.timestamp,
                    cases.item_text_fi,
                    cases.item_number
                FROM
                    agenda_items
                LEFT JOIN
                    cases on cases.event_id = (
                        select event_id from cases
                        where
                            cases.meeting_id = agenda_items.meeting_id
                            and
                            agenda_items.agenda_point = cases.case_number::int8
                            and
                            (cases.item_number = '0' or cases.item_number = '1')
                            order by item_number limit 1
                    )
                LEFT JOIN meeting_events on
                    cases.event_id = meeting_events.event_id
                WHERE agenda_items.meeting_id = @id AND language = @language
            ";
            var result = (await connection.QueryAsync<AgendaItem>(sqlQuery, new { @id, @language })).ToList();

            return result;
        }

        /// <summary>
        /// Retrieves all attachments for agenda items in a meeting.
        /// Returns attachments that match the language or have no language specified.
        /// </summary>
        /// <param name="id">The meeting identifier.</param>
        /// <param name="language">The language code for filtering attachments.</param>
        /// <returns>A list of agenda item attachments with file URIs and metadata.</returns>
        public async Task<List<AgendaItemAttachment>> FetchAgendaAttachmentsByMeetingId(string id, string language)
        {
            _logger.LogInformation($"FetchAgendaAttachmentsByMeetingId {id} {language}");
            
            var sqlQuery = @"
                SELECT
                    meeting_id,
                    agenda_point,
                    native_id,
                    title,
                    attachment_number,
                    publicity_class,
                    security_reasons,
                    type,
                    file_uri,
                    personal_data,
                    issued,
                    language
                FROM
                    agenda_item_attachments
                WHERE
                    meeting_id = @id
                    and
                    (language = @language or language is null)
            ";

            using var connection = await _connectionFactory.CreateOpenConnection();
            var result = (await connection.QueryAsync<AgendaItemAttachment>(sqlQuery, new { id, language })).ToList();

            return result;
        }

        /// <summary>
        /// Updates the HTML content and editor information for a specific agenda item.
        /// </summary>
        /// <param name="agendaItem">The agenda item containing updated HTML and editor information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpsertAgendaItemHtml(AgendaItem agendaItem)
        {
            _logger.LogInformation("UpsertAgendaItemHtml");

            using var connection = await _connectionFactory.CreateOpenConnection();
            
            var sqlQuery = @"
                UPDATE agenda_items SET 
                    html_content = @html,
                    editor_user_name = @editor
                WHERE
                    agenda_items.meeting_id = @meetingId
                    and
                    agenda_items.agenda_point = @agendaPoint
                    and
                    agenda_items.language = @language
            ;";

            await connection.ExecuteAsync(sqlQuery, new
            {
                html = agendaItem.Html,
                meetingId = agendaItem.MeetingID,
                agendaPoint = agendaItem.AgendaPoint,
                language = agendaItem.Language,
                editor = agendaItem.EditorUserName,
            });
        }

        /// <summary>
        /// Inserts or updates multiple agenda items using a batch upsert operation.
        /// Updates are performed based on meeting_id, agenda_point, and language.
        /// </summary>
        /// <param name="agendaItems">The list of agenda items to upsert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Inserts or updates agenda item attachments using a batch upsert operation.
        /// Updates are performed based on meeting_id, agenda_point, and attachment_number.
        /// </summary>
        /// <param name="attachments">The list of attachments to upsert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Inserts or updates agenda item PDF documents using upsert operations.
        /// One PDF per meeting/agenda point combination.
        /// </summary>
        /// <param name="decisionPdfs">The list of PDF attachments to upsert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Inserts or updates agenda item decision history PDF documents.
        /// One history PDF per meeting/agenda point combination.
        /// </summary>
        /// <param name="decisionHistoryPdfs">The list of decision history PDFs to upsert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

