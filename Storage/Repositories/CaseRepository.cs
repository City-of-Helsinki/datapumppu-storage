using Dapper;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Repositories
{
    /// <summary>
    /// Provides data access methods for case information management.
    /// </summary>
    public interface ICaseRepository
    {
        /// <summary>
        /// Inserts a new case or updates an existing case in the database.
        /// </summary>
        /// <param name="caseitem">The case to upsert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpsertCase(Case caseitem, IDbConnection connection, IDbTransaction transaction);
    }

    /// <summary>
    /// Implements case data access operations using Dapper for PostgreSQL queries.
    /// Handles case propositions and metadata storage with bilingual support (Finnish/Swedish).
    /// </summary>
    public class CaseRepository: ICaseRepository
    {

        private readonly ILogger<CaseRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the CaseRepository class.
        /// </summary>
        /// <param name="logger">Logger for diagnostic information.</param>
        public CaseRepository(ILogger<CaseRepository> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Inserts a new case or updates an existing case using an upsert operation.
        /// Updates are performed when a case with the same meeting_id, case_number, and item_number already exists.
        /// </summary>
        /// <param name="caseItem">The case entity to insert or update.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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
