using Storage.Actions;
using Storage.Events;
using Storage.Events.Providers;
using Storage.Mappers;
using Storage.Providers;
using Storage.Providers.Statistics;
using Storage.Repositories;
using Storage.Repositories.Migration;
using Storage.Repositories.Providers;
using Storage.Repositories.Statistics;

namespace Storage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddHealthChecks()
                .AddNpgSql(builder.Configuration["STORAGE_DB_CONNECTION_STRING"]);

            builder.Services.AddSingleton<IDatabaseConnectionFactory, DatabaseConnectionFactory>();
            builder.Services.AddSingleton<IKafkaClientFactory, KafkaClientFactory>();

            builder.Services.AddScoped<IMeetingsRepository, MeetingsRepository>();
            builder.Services.AddScoped<IAgendaItemsRepository, AgendaItemsRepository>();
            builder.Services.AddScoped<IDecisionsRepository, DecisionsRepository>();
            builder.Services.AddScoped<IDecisionsReadOnlyRepository, DecisionsRepository>();
            builder.Services.AddScoped<IEventsRepository, EventsRepository>();
            builder.Services.AddScoped<IStatementsRepository, StatementsRepository>();
            builder.Services.AddScoped<IMeetingSeatsRepository, MeetingSeatsRepository>();
            builder.Services.AddScoped<ICaseRepository, CaseRepository>();
            builder.Services.AddScoped<IPropositionsRepository, PropositionsRepository>();
            builder.Services.AddScoped<IPauseInfoRepository, PauseInfoRepository>();
            builder.Services.AddScoped<ISpeechTimerEventsRepository, SpeechTimerEventsRepository>();
            builder.Services.AddScoped<IRollCallRepository, RollCallRepository>();
            builder.Services.AddScoped<IPersonEventsRepository, PersonEventsRepository>();
            builder.Services.AddScoped<IVotingsRepository, VotingsRepository>();
            builder.Services.AddScoped<IAdminUsersRepository, AdminUsersRepository>();
            builder.Services.AddScoped<IVideoSyncRepository, VideoSyncRepository>();
            builder.Services.AddScoped<IStatementStatisticsRepository, StatementStatisticsRepository>();
            builder.Services.AddScoped<IVotingStatisticsRepository, VotingStatisticsRepository>();
            builder.Services.AddScoped<IPersonStatementStatisticsRepository, PersonStatementStatisticsRepository>();
            builder.Services.AddScoped<IParticipantsRepository, ParticipantsRepository>();

            builder.Services.AddScoped<ISeatsProvider, SeatsProvider>();
            builder.Services.AddScoped<IVotesProvider, VotesProvider>();
            builder.Services.AddScoped<IMeetingProvider, MeetingProvider>();
            builder.Services.AddScoped<IDecisionProvider, DecisionProvider>();
            builder.Services.AddScoped<IStatementProvider, StatementProvider>();
            builder.Services.AddScoped<IReservationsProvider, ReservationsProvider>();
            builder.Services.AddScoped<IStatementStatisticsProvider, StatementStatisticsProvider>();
            builder.Services.AddScoped<IVotingStatisticsProvider, VotingStatisticsProvider>();
            builder.Services.AddScoped<IPersonStatementStatisticsProvider, PersonStatementStatisticsProvider>();
            builder.Services.AddScoped<IParticipantStatisticsProvider, ParticipantStatisticsProvider>();

            builder.Services.AddScoped<IFullDecisionMapper, FullDecisionMapper>();

            builder.Services.AddScoped<IUpsertMeetingAction, UpsertMeetingAction>();
            builder.Services.AddScoped<IUpsertAgendaPointAction, UpsertAgendaPointAction>();
            builder.Services.AddScoped<IUpsertVideoSyncItemAction, UpsertVideoSyncItemAction>();
            builder.Services.AddScoped<IEventActions, EventActions>();

            builder.Services.AddScoped<IEventAction, UpdateMeetingStatusAction>();
            builder.Services.AddScoped<IEventAction, InsertEventAction>();
            builder.Services.AddScoped<IEventAction, UpdateVotingStatusAction>();
            builder.Services.AddScoped<IEventAction, UpdateStatementsAction>();
            builder.Services.AddScoped<IEventAction, UpdateMeetingSeatsAction>();
            builder.Services.AddScoped<IEventAction, UpsertCaseAction>();
            builder.Services.AddScoped<IEventAction, UpsertRollCallAction>();
            builder.Services.AddScoped<IEventAction, InsertStatementReservationAction>();
            builder.Services.AddScoped<IEventAction, InsertStartedStatementAction>();
            builder.Services.AddScoped<IEventAction, InsertPersonEventAction>();
            builder.Services.AddScoped<IEventAction, InsertPauseInfoAction>();
            builder.Services.AddScoped<IEventAction, InsertSpeechTimerEventAction>();
            builder.Services.AddScoped<IEventAction, InsertPropositionsEventAction>();
            builder.Services.AddScoped<IEventAction, InsertReplyReservationAction>();

            if (!string.IsNullOrEmpty(builder.Configuration["SB_CONNECTION_STRING"]))
            {
                builder.Services.AddHostedService<EventObserver>();
            }
            else if (!string.IsNullOrEmpty(builder.Configuration["KAFKA_BOOTSTRAP_SERVER"]))
            {
                builder.Services.AddHostedService<KafkaEventObserver>();
            }

            builder.Services.AddHostedService<DatabaseMigrationService>();
            builder.Services.AddHostedService<DatabaseCleaner>();

            builder.Services.AddLogging(options =>
            {
                options.AddSimpleConsole(c =>
                {
                    c.IncludeScopes = true;
                    c.SingleLine = false;
                    c.TimestampFormat = "dd.MM.yyyy HH:mm:ss ";
                });
            });

            var app = builder.Build();

            app.UseRouting();

            // Configure the HTTP request pipeline.

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthz");
                endpoints.MapHealthChecks("/readiness");
            });

            app.MapControllers();

            app.Run();
        }
    }
}