using Dapper;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Repositories
{
    /// <summary>
    /// Provides data access methods for proposition management.
    /// </summary>
    public interface IPropositionsRepository
    {
        /// <summary>
        /// Inserts a list of propositions into the database.
        /// </summary>
        /// <param name="propositions">The list of propositions to insert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InsertPropositions(List<Proposition> propositions, IDbConnection connection, IDbTransaction transaction);
    }

    /// <summary>
    /// Implements proposition data access operations using Dapper for PostgreSQL queries.
    /// Stores meeting propositions with bilingual text and metadata.
    /// </summary>
    public class PropositionsRepository : IPropositionsRepository
    {
        /// <summary>
        /// Inserts multiple propositions into the database in a single batch operation.
        /// </summary>
        /// <param name="propositions">The collection of proposition entities to insert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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
