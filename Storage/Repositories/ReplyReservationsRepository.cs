using Dapper;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Repositories
{
    public interface IReplyReservationsRepository
    {
        Task InsertReplyReservation(ReplyReservation replyReservation, IDbConnection connection, IDbTransaction transaction);
    }

    public class ReplyReservationsRepository: IReplyReservationsRepository
    {
        public Task InsertReplyReservation(ReplyReservation replyReservation, IDbConnection connection, IDbTransaction transaction)
        {
            var sqlQuery = @"INSERT INTO reply_reservations (meeting_id, event_id, person, additional_info_fi, additional_info_sv) values(
                @meetingId, 
                @eventId,
                @person,
                @additionalInfoFi,
                @additionalInfoSv
            ) ";

            return connection.ExecuteAsync(sqlQuery, replyReservation, transaction);
        }
    }
}
