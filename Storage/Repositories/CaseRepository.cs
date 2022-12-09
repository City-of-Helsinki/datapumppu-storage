using Dapper;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Repositories
{
    public interface ICaseRepository
    {
        Task InsertCase(Case caseitem, IDbConnection connection, IDbTransaction transaction);
    }

    public class CaseRepository: ICaseRepository
    {
        public Task InsertCase(Case caseItem, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"INSERT INTO cases (meeting_id, event_id, proposition_fi, proposition_sv, case_text, item_text, case_id) values(
                @meetingId, 
                @eventId,
                @propositionFi, 
                @propositionSv,
                @caseText,
                @itemText,
                @caseId
            ) ";
            sqlQuery += @"ON CONFLICT (meeting_id, case_id) DO UPDATE SET 
                event_id = @eventId,
                proposition_fi = @propositionFi,
                proposition_sv = @propositionSv,
                case_text = @caseText,
                item_text = @itemText
                WHERE cases.meeting_id = @meetingId and cases.case_id = @caseId
            ;";

            return connection.ExecuteAsync(sqlQuery, caseItem, transaction);
        }
    }
}
