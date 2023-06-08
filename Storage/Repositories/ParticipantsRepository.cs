using Dapper;
using Storage.Repositories.Providers;

namespace Storage.Repositories
{
    public interface IParticipantsRepository
    {
        Task<List<string>> GetParticipants(int year);
    }

    public class ParticipantsRepository : IParticipantsRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public ParticipantsRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<string>> GetParticipants(int year)
        {
            var meetingId = $"02900{year}%";

            var query = @"
                select distinct
                    person
                from
                    meeting_seat_updates
                join meeting_seats
                    on meeting_seat_updates.id = meeting_seats.meeting_seat_update_id
                where
                    meeting_id like @meetingId";

            using var connection = await _connectionFactory.CreateOpenConnection();
            return (await connection.QueryAsync<string>(query, new { meetingId })).ToList();
        }
    }
}
