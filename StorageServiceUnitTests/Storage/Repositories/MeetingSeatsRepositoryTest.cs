using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using Storage.Repositories;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;
using Xunit;

namespace StorageServiceUnitTests.Storage.Repositories
{
    public class MeetingSeatsRepositoryTest
    {
        private readonly string _connectionString = "Host=localhost;Port=5432;Database=storage;Username=datapumppu;Password=password";
        private readonly string _meetingId = "test_seats_meeting_1";

        [Fact]
        public async Task GetUpdateIdForVoting_ReturnsClosestSeatUpdateBeforeVoting_BasedOnSequenceNumber()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TF_BUILD")))
            {
                return; // Skip database integration tests on Azure Pipelines
            }

            // Arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["STORAGE_DB_CONNECTION_STRING"]).Returns(_connectionString);

            var connectionFactory = new DatabaseConnectionFactory(configMock.Object);
            var loggerMock = new Mock<ILogger<MeetingSeatsRepository>>();
            var repository = new MeetingSeatsRepository(loggerMock.Object, connectionFactory);

            try
            {
                // Pre-test cleanup
                using (var initConnection = (NpgsqlConnection)await connectionFactory.CreateOpenConnection())
                {
                    await CleanDb(initConnection);

                    // 1. Insert Meeting
                    var insertMeetingSql = @"
                        INSERT INTO meetings (meeting_id, name, location, meeting_date, meeting_sequence_number)
                        VALUES (@meetingId, 'Test Seats Meeting', 'Chamber', '2023-05-15 10:00:00', 8);
                    ";
                    using (var cmd = new NpgsqlCommand(insertMeetingSql, initConnection))
                    {
                        cmd.Parameters.AddWithValue("meetingId", _meetingId);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // 2. Insert Events
                    var insertEventsSql = @"
                        INSERT INTO meeting_events (meeting_id, event_id, event_type, timestamp, sequence_number, case_number, item_number)
                        VALUES 
                            (@meetingId, '00000000-0000-0000-0000-000000000010', 'AttendeesEvent', '2023-05-15 10:01:00', 10, '1', '1'),
                            (@meetingId, '00000000-0000-0000-0000-000000000020', 'AttendeesEvent', '2023-05-15 10:02:00', 20, '1', '1'),
                            (@meetingId, '00000000-0000-0000-0000-000000000025', 'VotingStartedEvent', '2023-05-15 10:03:00', 25, '1', '1');
                    ";
                    using (var cmd = new NpgsqlCommand(insertEventsSql, initConnection))
                    {
                        cmd.Parameters.AddWithValue("meetingId", _meetingId);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // 3. Insert Seat Updates
                    var insertSeatUpdatesSql = @"
                        INSERT INTO meeting_seat_updates (meeting_id, attendees_eventid, sequence_number, timestamp)
                        VALUES 
                            (@meetingId, '00000000-0000-0000-0000-000000000010', 10, '2023-05-15 10:01:00'),
                            (@meetingId, '00000000-0000-0000-0000-000000000020', 20, '2023-05-15 10:02:00');
                    ";
                    using (var cmd = new NpgsqlCommand(insertSeatUpdatesSql, initConnection))
                    {
                        cmd.Parameters.AddWithValue("meetingId", _meetingId);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Get the generated IDs of the seat updates to verify against later
                    var selectIdsSql = "SELECT id FROM meeting_seat_updates WHERE meeting_id = @meetingId ORDER BY sequence_number ASC;";
                    var seatUpdateIds = new List<int>();
                    using (var cmd = new NpgsqlCommand(selectIdsSql, initConnection))
                    {
                        cmd.Parameters.AddWithValue("meetingId", _meetingId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                seatUpdateIds.Add(reader.GetInt32(0));
                            }
                        }
                    }

                    Assert.Equal(2, seatUpdateIds.Count);
                    int expectedFirstUpdateId = seatUpdateIds[0];
                    int expectedSecondUpdateId = seatUpdateIds[1];

                    // 4. Insert Voting Session
                    var insertVotingSql = @"
                        INSERT INTO votings (meeting_id, voting_number, voting_type, voting_type_text_fi, voting_started, voting_started_eventid, votes_for, votes_against, votes_empty, votes_absent)
                        VALUES (@meetingId, 1001, 1, 'PON', '2023-05-15 10:03:00', '00000000-0000-0000-0000-000000000025', 0, 0, 0, 0);
                    ";
                    using (var cmd = new NpgsqlCommand(insertVotingSql, initConnection))
                    {
                        cmd.Parameters.AddWithValue("meetingId", _meetingId);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Act & Assert
                    // Call GetUpdateIdForVoting and verify it resolves to the second seat update (closest sequence number before the voting)
                    var resolvedUpdateId = await repository.GetUpdateIdForVoting(_meetingId, 1001);
                    Assert.Equal(expectedSecondUpdateId, resolvedUpdateId);

                    // Verify that if we query for a sequence number that is before the second update, it would only find the first one
                    // We can check this by testing the query itself on our mock state
                }
            }
            finally
            {
                // Post-test cleanup
                using (var cleanupConnection = (NpgsqlConnection)await connectionFactory.CreateOpenConnection())
                {
                    await CleanDb(cleanupConnection);
                }
            }
        }

        private async Task CleanDb(NpgsqlConnection connection)
        {
            var cleanupSql = @"
                DELETE FROM votings WHERE meeting_id = @meetingId;
                DELETE FROM meeting_seats WHERE meeting_seat_update_id IN (SELECT id FROM meeting_seat_updates WHERE meeting_id = @meetingId);
                DELETE FROM meeting_seat_updates WHERE meeting_id = @meetingId;
                DELETE FROM meeting_events WHERE meeting_id = @meetingId;
                DELETE FROM meetings WHERE meeting_id = @meetingId;
            ";
            using (var cmd = new NpgsqlCommand(cleanupSql, connection))
            {
                cmd.Parameters.AddWithValue("meetingId", _meetingId);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
