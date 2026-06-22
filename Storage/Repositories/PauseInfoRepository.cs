using Dapper;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Repositories
{
    /// <summary>
    /// Provides data access methods for meeting pause information.
    /// </summary>
    public interface IPauseInfoRepository
    {
        /// <summary>
        /// Inserts pause information for a meeting event.
        /// </summary>
        /// <param name="pauseInfo">The pause information to insert.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InsertPauseInfo(PauseInfo pauseInfo, IDbConnection connection, IDbTransaction transaction);
    }

    /// <summary>
    /// Implements pause information data access operations using Dapper for PostgreSQL queries.
    /// Records meeting pause events with associated descriptive information.
    /// </summary>
    public class PauseInfoRepository : IPauseInfoRepository
    {
        /// <summary>
        /// Inserts pause information into the database for a specific meeting event.
        /// </summary>
        /// <param name="pauseInfo">The pause info entity containing meeting ID, event ID, and pause details.</param>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="transaction">The database transaction to participate in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task InsertPauseInfo(PauseInfo pauseInfo, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"INSERT INTO pause_infos (meeting_id, event_id, info) values(
                @meetingId, 
                @eventId,
                @info
            );";
            
            return connection.ExecuteAsync(sqlQuery, pauseInfo, transaction);
        }
    }
}
