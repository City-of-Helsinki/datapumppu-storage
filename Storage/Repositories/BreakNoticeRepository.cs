using Dapper;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Repositories
{
    public interface IBreakNoticeRepository
    {
        Task InsertBreakNotice(BreakNotice breakNotice, IDbConnection connection, IDbTransaction transaction);
    }

    public class BreakNoticeRepository : IBreakNoticeRepository
    {
        public Task InsertBreakNotice(BreakNotice breakNotice, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"INSERT INTO break_notices (meeting_id, event_id, notice) values(
                @meetingId, 
                @eventId,
                @notice
            );";
            
            return connection.ExecuteAsync(sqlQuery, breakNotice, transaction);
        }
    }
}
