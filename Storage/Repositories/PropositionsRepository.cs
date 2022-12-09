using Dapper;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Repositories
{
    public interface IPropositionsRepository
    {
        Task InsertPropositions(List<Proposition> propositions, IDbConnection connection, IDbTransaction transaction);
    }

    public class PropositionsRepository : IPropositionsRepository
    {
        public Task InsertPropositions(List<Proposition> propositions, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"INSERT INTO propositions (meeting_id, event_id, text_fi, text_sv, person_fi, person_sv, type, type_text_fi, type_text_sv) values(
                @meetingId, 
                @eventId,
                @textFi, 
                @textSv, 
                @personFi,
                @personSv,
                @type,
                @typeTextFi,
                @typeTextSv
            ) ";

            return connection.ExecuteAsync(sqlQuery, propositions.Select(item => new {
                meetingId = item.MeetingID,
                eventId = item.EventID,
                textFi = item.TextFI,
                textSv = item.TextSV,
                personFi = item.PersonFI,
                personSv = item.PersonSV,
                type = item.Type,
                typeTextFi = item.TypeTextFI,
                typeTextSv = item.TypeTextSV
            }), transaction);
        }
    }
}
