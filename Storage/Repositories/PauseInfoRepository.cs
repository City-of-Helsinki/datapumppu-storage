using Dapper;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Repositories
{
    public interface IPauseInfoRepository
    {
        Task InsertPauseInfo(PauseInfo pauseInfo, IDbConnection connection, IDbTransaction transaction);
    }

    public class PauseInfoRepository : IPauseInfoRepository
    {
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
