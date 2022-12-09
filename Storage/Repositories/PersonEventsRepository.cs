using System.Data;
using Dapper;
using Storage.Repositories.Models;

namespace Storage.Repositories
{
    public interface IPersonEventsRepository
    {
        Task InsertPersonEvent(PersonEvent personEvent, IDbConnection connection, IDbTransaction transaction);
    }

    public class PersonEventsRepository: IPersonEventsRepository
    {
        private readonly ILogger<PersonEventsRepository> _logger;

        public PersonEventsRepository(ILogger<PersonEventsRepository> logger)
        {
            _logger = logger;
        }

        public Task InsertPersonEvent(PersonEvent personEvent, IDbConnection connection, IDbTransaction transaction)
        {
            _logger.LogInformation("Executing InsertPersonEvent()");
            var sqlQuery = @"insert into person_events (meeting_id, event_id, timestamp, person_fi, person_sv, event_type, seat_id) values(               
                @meetingId,
                @eventId,
                @timestamp,
                @personFi,
                @personSv,
                @eventType,
                @seatId
            )";

            return connection.ExecuteAsync(sqlQuery, personEvent, transaction);
        }
    }
}
