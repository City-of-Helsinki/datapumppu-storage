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
    public class StatementsRepositoryTest
    {
        private readonly string _connectionString = "Host=localhost;Port=5432;Database=storage;Username=datapumppu;Password=password";

        [Fact]
        public async Task StatementsRepository_QueriesAndCommands_ExecuteSuccessfullyAndReturnExpectedResults()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["STORAGE_DB_CONNECTION_STRING"]).Returns(_connectionString);

            var connectionFactory = new DatabaseConnectionFactory(configMock.Object);
            var loggerMock = new Mock<ILogger<StatementsRepository>>();
            var repository = new StatementsRepository(loggerMock.Object, connectionFactory);

            try
            {
                // Pre-test cleanup of any previous test leftovers
                using (var initConnection = (NpgsqlConnection)await connectionFactory.CreateOpenConnection())
                {
                    var cleanupSql = @"
                        DELETE FROM statements WHERE meeting_id IN ('test_meeting_123', 'test_meeting_reservations_123');
                        DELETE FROM meeting_events WHERE meeting_id IN ('test_meeting_123', 'test_meeting_reservations_123');
                        DELETE FROM agenda_items WHERE meeting_id IN ('test_meeting_123', 'test_meeting_reservations_123');
                        DELETE FROM meetings WHERE meeting_id IN ('test_meeting_123', 'test_meeting_reservations_123');
                    ";
                    using (var cmd = new NpgsqlCommand(cleanupSql, initConnection))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Insert fresh test data
                    var insertSql = @"
                        INSERT INTO meetings (meeting_id, name, location, meeting_date, meeting_sequence_number)
                        VALUES ('test_meeting_123', 'Test Meeting', 'Location', '2023-05-15 10:00:00', 1);

                        INSERT INTO agenda_items (meeting_id, agenda_point, title, language)
                        VALUES ('test_meeting_123', 1, 'Agenda 1', 'fi');

                        INSERT INTO meeting_events (meeting_id, event_id, event_type, timestamp, sequence_number, case_number, item_number)
                        VALUES ('test_meeting_123', '00000000-0000-0000-0000-000000000001', 'SpeechTimerEvent', '2023-05-15 10:05:00', 1, '1', '1');

                        INSERT INTO statements (meeting_id, event_id, person, started, ended, speech_type, duration_seconds, additional_info_fi, additional_info_sv)
                        VALUES ('test_meeting_123', '00000000-0000-0000-0000-000000000001', 'Test Person', '2023-05-15 10:05:00', '2023-05-15 10:10:00', 1, 300, 'puhe fi', 'tal sv');

                        INSERT INTO meetings (meeting_id, name, location, meeting_date, meeting_sequence_number)
                        VALUES ('test_meeting_reservations_123', 'Test Meeting Reservations', 'Location', '2023-05-15 10:00:00', 1);
                    ";
                    using (var cmd = new NpgsqlCommand(insertSql, initConnection))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                // Act
                // 1. Test query by name (parameterized SQL injection-proof query)
                var resultsByName = await repository.GetStatementsByPersonOrDate(new List<string> { "Test Person" }, null, null, "fi");

                // 2. Test query by date range
                var resultsByDate = await repository.GetStatementsByPersonOrDate(new List<string>(), new DateTime(2023, 5, 14), new DateTime(2023, 5, 16), "fi");

                // 3. Test reservations querying
                var reservations = await repository.GetStatementReservations("test_meeting_reservations_123", "1");
                var replyReservations = await repository.GetReplyReservations("test_meeting_reservations_123", "1");

                // Assert
                Assert.NotNull(resultsByName);
                Assert.NotEmpty(resultsByName);
                Assert.Equal("Test Person", resultsByName[0].Person);

                Assert.NotNull(resultsByDate);
                Assert.NotEmpty(resultsByDate);
                Assert.Equal("Test Person", resultsByDate[0].Person);

                Assert.NotNull(reservations);
                Assert.NotNull(replyReservations);
            }
            finally
            {
                // Post-test cleanup to leave the database completely clean
                using (var cleanupConnection = (NpgsqlConnection)await connectionFactory.CreateOpenConnection())
                {
                    var cleanupSql = @"
                        DELETE FROM statements WHERE meeting_id IN ('test_meeting_123', 'test_meeting_reservations_123');
                        DELETE FROM meeting_events WHERE meeting_id IN ('test_meeting_123', 'test_meeting_reservations_123');
                        DELETE FROM agenda_items WHERE meeting_id IN ('test_meeting_123', 'test_meeting_reservations_123');
                        DELETE FROM meetings WHERE meeting_id IN ('test_meeting_123', 'test_meeting_reservations_123');
                    ";
                    using (var cmd = new NpgsqlCommand(cleanupSql, cleanupConnection))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }
    }
}
