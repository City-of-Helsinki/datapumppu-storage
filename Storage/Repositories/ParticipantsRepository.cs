using Dapper;
using Storage.Repositories.Providers;

namespace Storage.Repositories
{
    /// <summary>
    /// Provides data access methods for meeting participant information.
    /// </summary>
    public interface IParticipantsRepository
    {
        /// <summary>
        /// Retrieves a distinct list of all participants who attended meetings in the specified year.
        /// </summary>
        /// <param name="year">The year to query participants for.</param>
        /// <returns>A list of participant names.</returns>
        Task<List<string>> GetParticipants(int year);
    }

    /// <summary>
    /// Implements participant data access operations using Dapper for PostgreSQL queries.
    /// Retrieves participant information from meeting seat allocations.
    /// </summary>
    public class ParticipantsRepository : IParticipantsRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        /// <summary>
        /// Initializes a new instance of the ParticipantsRepository class.
        /// </summary>
        /// <param name="connectionFactory">Factory for creating database connections.</param>
        public ParticipantsRepository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Retrieves all distinct participants from meetings in the specified year.
        /// Participants are identified from seat allocation records.
        /// </summary>
        /// <param name="year">The year to query (e.g., 2024).</param>
        /// <returns>A list of unique participant names.</returns>
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
