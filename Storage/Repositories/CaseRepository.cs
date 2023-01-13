using Dapper;
using Newtonsoft.Json;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Repositories
{
    public interface ICaseRepository
    {
        Task UpsertCase(Case caseitem, IDbConnection connection, IDbTransaction transaction);
    }

    public class CaseRepository: ICaseRepository
    {

        private readonly ILogger<CaseRepository> _logger;

        public CaseRepository(ILogger<CaseRepository> logger)
        {
            _logger = logger;
        }

        public Task UpsertCase(Case caseItem, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Executing UpsertCase()");
           
            var sqlQuery = @"INSERT INTO cases (meeting_id, case_number, item_number, event_id, proposition_fi, proposition_sv, case_text_fi, case_text_sv, item_text_fi, item_text_sv, identifier) values(
                @meetingId, 
                @caseNumber,
                @itemNumber,
                @eventId,
                @propositionFi, 
                @propositionSv,
                @caseTextFi,
                @caseTextSv,
                @itemTextFi,
                @itemTextSv,
                @identifier
            ) ";
            sqlQuery += @"ON CONFLICT (meeting_id, case_number, item_number) DO UPDATE SET 
                event_id = @eventId,
                proposition_fi = @propositionFi,
                proposition_sv = @propositionSv,
                case_text_fi = @caseTextFi,
                case_text_sv = @caseTextSv,
                item_text_fi = @itemTextFi,
                item_text_sv = @itemTextSv,
                identifier = @identifier
                WHERE cases.meeting_id = @meetingId and cases.case_number = @caseNumber and cases.item_number = @itemNumber
            ;";

            return connection.ExecuteAsync(sqlQuery, caseItem, transaction);
        }
    }
}
