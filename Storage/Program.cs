using Storage.Actions;
using Storage.Events;
using Storage.Providers;
using Storage.Repositories;
using Storage.Repositories.Providers;

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
                .AddSqlServer(builder.Configuration["Database:ConnectionString"]);

            builder.Services.AddSingleton<IDatabaseConnectionFactory, DatabaseConnectionFactory>();

            builder.Services.AddScoped<IMeetingsRepository, MeetingsRepository>();
            builder.Services.AddScoped<IAgendaItemsRepository, AgendaItemsRepository>();
            builder.Services.AddScoped<IDecisionsRepository, DecisionsRepository>();
            builder.Services.AddScoped<IEventsRepository, EventsRepository>();
            builder.Services.AddScoped<ISpeakingTurnsRepository, SpeakingTurnsRepository>();
            builder.Services.AddScoped<IMeetingSeatsRepository, MeetingSeatsRepository>();
            builder.Services.AddScoped<ICaseRepository, CaseRepository>();
            builder.Services.AddScoped<IPropositionsRepository, PropositionsRepository>();
            builder.Services.AddScoped<IBreakNoticeRepository, BreakNoticeRepository>();
            builder.Services.AddScoped<ISpeechTimerEventsRepository, SpeechTimerEventsRepository>();
            builder.Services.AddScoped<IRollCallRepository, RollCallRepository>();
            builder.Services.AddScoped<IPersonEventsRepository, PersonEventsRepository>();
            builder.Services.AddScoped<IReplyReservationsRepository, ReplyReservationsRepository>();
            builder.Services.AddScoped<IUpsertMeetingAction, UpsertMeetingAction>();
            builder.Services.AddScoped<IVotingsRepository, VotingsRepository>();
            builder.Services.AddScoped<IMeetingProvider, MeetingProvider>();
            builder.Services.AddScoped<IEventActions, EventActions>();
            builder.Services.AddScoped<IEventAction, InsertEventAction>();
            builder.Services.AddScoped<IEventAction, UpdateMeetingStatusAction>();
            builder.Services.AddScoped<IEventAction, UpdateVotingStatusAction>();
            builder.Services.AddScoped<IEventAction, UpdateSpeakingTurnsAction>();
            builder.Services.AddScoped<IEventAction, UpdateMeetingSeatsAction>();
            builder.Services.AddScoped<IEventAction, InsertCaseAction>();
            builder.Services.AddScoped<IEventAction, UpsertRollCallAction>();
            builder.Services.AddScoped<IEventAction, InsertSpeakingTurnReservationAction>();
            builder.Services.AddScoped<IEventAction, InsertStartedSpeakingTurnAction>();
            builder.Services.AddScoped<IEventAction, InsertPersonEventAction>();
            builder.Services.AddScoped<IEventAction, InsertBreakNoticeAction>();
            builder.Services.AddScoped<IEventAction, InsertSpeechTimerEventAction>();
            builder.Services.AddScoped<IEventAction, InsertPropositionsEventAction>();
            builder.Services.AddScoped<IEventAction, InsertReplyReservationAction>();


            builder.Services.AddHostedService<EventObserver>();

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