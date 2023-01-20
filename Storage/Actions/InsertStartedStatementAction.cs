using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    public class InsertStartedStatementAction : IEventAction
    {
        public List<EventType> EventTypes { get; } = new()
            { EventType.StatementStarted };

        private readonly IStatementsRepository _statementsRepository;

        public InsertStartedStatementAction(IStatementsRepository statementsRepository)
        {
            _statementsRepository = statementsRepository;
        }

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var statementStartedDto = eventBody.ToObjectFromJson<StatementStartedEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<StatementStartedEventDTO, StartedStatement>()
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var startedStatement = mapper.Map<StartedStatement>(statementStartedDto);

            return _statementsRepository.InsertStartedStatement(startedStatement, connection, transaction);
        }
    }
}