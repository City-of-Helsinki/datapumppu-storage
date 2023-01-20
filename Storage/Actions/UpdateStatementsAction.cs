using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    public class UpdateStatementsAction : IEventAction
    {
        public List<EventType> EventTypes { get; } = new()
            { EventType.Statements };

        private readonly IStatementsRepository _statementsRepository;

        public UpdateStatementsAction(IStatementsRepository statementsRepository)
        {
            _statementsRepository = statementsRepository;
        }

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var statementsEventDto = eventBody.ToObjectFromJson<StatementsEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<StatementDTO, Statement>()
                    .ForMember(dest => dest.MeetingID, opt => opt.MapFrom(x => statementsEventDto.MeetingID))
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId))
                    .ForMember(dest => dest.Started, opt => opt.MapFrom(src => src.StartTime))
                    .ForMember(dest => dest.Ended, opt => opt.MapFrom(src => src.EndTime))
                    .ForMember(dest => dest.CaseNumber, opt => opt.Ignore())
                    .ForMember(dest => dest.Title, opt => opt.Ignore())
                    .ForMember(dest => dest.DurationSeconds, opt => opt.MapFrom(src => src.Duration));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var statements = statementsEventDto.Statements.Select(statementDto => mapper.Map<Statement>(statementDto)).ToList();

            return _statementsRepository.UpsertStatements(statements, connection, transaction);
        }
    }
}
