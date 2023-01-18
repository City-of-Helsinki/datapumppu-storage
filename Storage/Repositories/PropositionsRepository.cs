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
            var sqlQuery = @"INSERT INTO propositions (meeting_id, event_id, text_fi, text_sv, person, type, type_text_fi, 
                type_text_sv, additional_info_fi, additional_info_sv) values(
                @meetingId, 
                @eventId,
                @textFi, 
                @textSv, 
                @person,
                @type,
                @typeTextFi,
                @typeTextSv,
                @additionalInfoFi,
                @additionalInfoSv
            ) ";

            return connection.ExecuteAsync(sqlQuery, propositions.Select(item => new {
                meetingId = item.MeetingID,
                eventId = item.EventID,
                textFi = item.TextFI,
                textSv = item.TextSV,
                person = item.Person,
                type = item.Type,
                typeTextFi = item.TypeTextFI,
                typeTextSv = item.TypeTextSV,
                additionalInfoFi = item.AdditionalInfoFI,
                additionalInfoSv = item.AdditionalInfoSV,
            }), transaction);
        }
    }
}
