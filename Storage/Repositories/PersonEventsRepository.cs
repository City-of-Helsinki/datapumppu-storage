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
            var sqlQuery = @"insert into person_events (meeting_id, event_id, timestamp, person, event_type, 
                seat_id, additional_info_fi, additional_info_sv) values(               
                @meetingId,
                @eventId,
                @timestamp,
                @person,
                @eventType,
                @seatId,
                @additionalInfoFi,
                @additionalInfoSv
            )";

            return connection.ExecuteAsync(sqlQuery, personEvent, transaction);
        }
    }
}
