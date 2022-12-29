using Storage.Repositories.Models;
using System.Data;
using Dapper;
using Storage.Repositories.Providers;
using Storage.Providers.DTOs;
using System.Text.Json;
using Microsoft.Azure.ServiceBus;

namespace Storage.Repositories
{
    public interface IDecisionsRepository
    {
        Task UpsertDecisions(List<Decision> decisions, IDbConnection connection, IDbTransaction transaction);

        Task UpsertDecisionAttachments(List<DecisionAttachment> attachments, IDbConnection connection, IDbTransaction transaction);

        Task UpsertDecisionPdfs(List<DecisionAttachment> decisionPdfs, IDbConnection connection, IDbTransaction transaction);

        Task UpsertDecisionHistoryPdfs(List<DecisionAttachment> decisionHistoryPdfs, IDbConnection connection, IDbTransaction transaction);
    }

    public interface IDecisionsReadOnlyRepository
    {
        Task<List<FullDecision>> FetchDecisionsByMeetingId(string id, string language);

        Task<FullDecision?> FetchDecisionsByCaseIdLabel(string caseLabelId, string language);
    }

    public class DecisionsRepository: IDecisionsRepository, IDecisionsReadOnlyRepository
    {
        private readonly ILogger<DecisionsRepository> _logger;
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public DecisionsRepository(ILogger<DecisionsRepository> logger, IDatabaseConnectionFactory connectionFactory)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        public async Task<FullDecision?> FetchDecisionsByCaseIdLabel(string caseLabelId, string language)
        {
            using var connection = await _connectionFactory.CreateOpenConnection();
            var sqlQuery = @"
                SELECT * FROM decisions
                WHERE case_id_label = @caseLabelId AND language = @language;
            ";

            var decision = (await connection.QueryAsync<Decision>(sqlQuery, new { @caseLabelId, @language })).FirstOrDefault();
            if (decision == null)
            {
                return null;
            }

            var attachments = await FetchDecisionAttachments(decision.NativeId);
            var pdf = await FetchDecisionPdf(decision.NativeId);
            var historyPdf = await FetchDecisionHistoryPdf(decision.NativeId);

            return new FullDecision()
            {
                Decision = decision,
                Attachments = attachments,
                Pdf = pdf,
                DecisionHistoryPdf = historyPdf
            };
        }

        public async Task<List<FullDecision>> FetchDecisionsByMeetingId(string id, string language)
        {
            using var connection = await _connectionFactory.CreateOpenConnection();
            var sqlQuery = @"
                SELECT * FROM decisions
                WHERE meeting_id = @id AND language = @language;
            ";

            var decisions = (await connection.QueryAsync<Decision>(sqlQuery, new { @id, @language })).ToList();
            var result = new List<FullDecision>();

            foreach(var decision in decisions)
            {
                var attachments = await FetchDecisionAttachments(decision.NativeId);
                var pdf = await FetchDecisionPdf(decision.NativeId);
                var historyPdf = await FetchDecisionHistoryPdf(decision.NativeId);

                var fullDecision = new FullDecision()
                {
                    Decision = decision,
                    Attachments = attachments,
                    Pdf = pdf,
                    DecisionHistoryPdf = historyPdf
                };

                result.Add(fullDecision);
            }

            return result;
        }

        private async Task<List<DecisionAttachment>> FetchDecisionAttachments(string nativeId)
        {
            using var connection = await _connectionFactory.CreateOpenConnection();
            var sqlQuery = @"
                SELECT * FROM decision_attachments
                WHERE decision_id = @nativeId
            ";

            return (await connection.QueryAsync<DecisionAttachment>(sqlQuery, new { @nativeId })).ToList();
        }

        private async Task<DecisionAttachment> FetchDecisionPdf(string nativeId)
        {
            using var connection = await _connectionFactory.CreateOpenConnection();
            var sqlQuery = @"
                SELECT * FROM decision_pdfs
                WHERE decision_id = @nativeId
            ";

            return (await connection.QueryAsync<DecisionAttachment>(sqlQuery, new { @nativeId })).SingleOrDefault();
        }

        private async Task<DecisionAttachment?> FetchDecisionHistoryPdf(string nativeId)
        {
            using var connection = await _connectionFactory.CreateOpenConnection();
            var sqlQuery = @"
                SELECT * FROM decision_history_pdfs
                WHERE decision_id = @nativeId
            ";
        
            return (await connection.QueryAsync<DecisionAttachment>(sqlQuery, new { @nativeId })).SingleOrDefault();
        }

        public Task UpsertDecisions(List<Decision> decisions, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Upserting decisions");
            var sqlQuery = @"INSERT INTO decisions (meeting_id, native_id, title, case_id_label, case_id, section, 
                html, history_html, motion, classification_code, classification_title, language) values(
                @meetingId,
                @nativeId,
                @title,
                @caseIdLabel,
                @caseId,
                @section,
                @html,
                @historyHtml,
                @motion,
                @classificationCode,
                @classificationTitle,
                @language
            ) ";
            sqlQuery += @"ON CONFLICT (native_id) DO UPDATE SET 
                title = @title,
                case_id_label = @caseIdLabel,
                case_id = @caseId,
                section = @section,
                html = @html,
                history_html = @historyHtml,
                motion = @motion,
                classification_code = @classificationCode,
                classification_title = @classificationTitle,
                language = @language
                WHERE decisions.native_id = @nativeId
            ;";

            return connection.ExecuteAsync(sqlQuery, decisions.Select(item => new
            {
                meetingId = item.MeetingID,
                nativeId = item.NativeId,
                title = item.Title,
                caseIdLabel = item.CaseIDLabel,
                caseId = item.CaseID,
                section = item.Section,
                html = item.Html,
                historyHtml = item.DecisionHistoryHtml,
                motion = item.Motion,
                classificationCode = item.ClassificationCode,
                classificationTitle = item.ClassificationTitle,
                language = item.Language
            }), transaction);
        }

        public Task UpsertDecisionAttachments(List<DecisionAttachment> attachments,
            IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Upserting decision attachments");
            var sqlQuery = @"INSERT INTO decision_attachments (decision_id, native_id, title, attachment_number,
                publicity_class, security_reasons, type, file_uri, language, personal_data, issued) values(
                @decisionId,
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
            sqlQuery += @"ON CONFLICT (decision_id, attachment_number) DO UPDATE SET 
                title = @title,
                publicity_class = @publicityClass,
                security_reasons = @securityReasons,
                type = @type,
                file_uri = @fileUri,
                language = @language,
                personal_data = @personalData,
                issued = @issued
                WHERE decision_attachments.decision_id = @decisionId and decision_attachments.attachment_number = @attachmentNumber
            ;";

            return connection.ExecuteAsync(sqlQuery, attachments.Select(item => new
            {
                decisionId = item.DecisionId,
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

        public Task UpsertDecisionPdfs(List<DecisionAttachment> decisionPdfs,
            IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Upserting decision pdfs");
            var sqlQuery = @"INSERT INTO decision_pdfs (decision_id, native_id, title, attachment_number, publicity_class,
                security_reasons, type, file_uri, language, personal_data, issued) values(
                @decisionId,
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
            sqlQuery += @"ON CONFLICT (decision_id) DO UPDATE SET 
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
                WHERE decision_pdfs.decision_id = @decisionId
            ;";

            return connection.ExecuteAsync(sqlQuery, decisionPdfs.Select(item => new
            {
                decisionId = item.DecisionId,
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

        public Task UpsertDecisionHistoryPdfs(List<DecisionAttachment> decisionHistoryPdfs,
            IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Upserting decision history pdfs");
            var sqlQuery = @"INSERT INTO decision_history_pdfs (decision_id, native_id, title, attachment_number, publicity_class, 
                security_reasons, type, file_uri, language, personal_data, issued) values(
                @decisionId,
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
            sqlQuery += @"ON CONFLICT (decision_id) DO UPDATE SET 
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
                WHERE decision_history_pdfs.decision_id = @decisionId
            ;";

            return connection.ExecuteAsync(sqlQuery, decisionHistoryPdfs.Select(item => new
            {
                decisionId = item.DecisionId,
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
