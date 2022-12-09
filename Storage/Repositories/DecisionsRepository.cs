using Storage.Repositories.Models;
using System.Data;
using Dapper;

namespace Storage.Repositories
{
    public interface IDecisionsRepository
    {
        Task UpsertDecisions(List<Decision> decisions, IDbConnection connection, IDbTransaction transaction);

        Task UpsertDecisionAttachments(List<DecisionAttachment> attachments, IDbConnection connection, IDbTransaction transaction);

        Task UpsertDecisionPdfs(List<DecisionAttachment> decisionPdfs, IDbConnection connection, IDbTransaction transaction);

        Task UpsertDecisionHistoryPdfs(List<DecisionAttachment> decisionHistoryPdfs, IDbConnection connection, IDbTransaction transaction);
    }

    public class DecisionsRepository: IDecisionsRepository
    {
        private readonly ILogger<DecisionsRepository> _logger;

        public DecisionsRepository(ILogger<DecisionsRepository> logger)
        {
            _logger = logger;
        }

        public Task UpsertDecisions(List<Decision> decisions, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Upserting decisions");
            var sqlQuery = @"INSERT INTO decisions (native_id, title, case_id_label, case_id, section, html, history_html, motion, classification_code, classification_title) values(
                @nativeId,
                @title,
                @caseIdLabel,
                @caseId,
                @section,
                @html,
                @historyHtml,
                @motion,
                @classificationCode,
                @classificationTitle
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
                classification_title = @classificationTitle
                WHERE decisions.native_id = @nativeId
            ;";

            return connection.ExecuteAsync(sqlQuery, decisions.Select(item => new
            {
                nativeId = item.NativeId,
                title = item.Title,
                caseIdLabel = item.CaseIDLabel,
                caseId = item.CaseID,
                section = item.Section,
                html = item.Html,
                historyHtml = item.DecisionHistoryHtml,
                motion = item.Motion,
                classificationCode = item.ClassificationCode,
                classificationTitle = item.ClassificationTitle
            }), transaction);
        }

        public Task UpsertDecisionAttachments(List<DecisionAttachment> attachments,
            IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Upserting decision attachments");
            var sqlQuery = @"INSERT INTO decision_attachments (decision_id, native_id, title, attachment_number, publicity_class, security_reasons, type, file_uri, language, personal_data, issued) values(
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
            var sqlQuery = @"INSERT INTO decision_pdfs (decision_id, native_id, title, attachment_number, publicity_class, security_reasons, type, file_uri, language, personal_data, issued) values(
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
            var sqlQuery = @"INSERT INTO decision_history_pdfs (decision_id, native_id, title, attachment_number, publicity_class, security_reasons, type, file_uri, language, personal_data, issued) values(
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
