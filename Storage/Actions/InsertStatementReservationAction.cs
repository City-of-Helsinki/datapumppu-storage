using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    public class InsertStatementReservationAction : IEventAction
    {
        public List<EventType> EventTypes { get; } = new()
            { EventType.StatementReservation };

        private readonly IStatementsRepository _statementsRepository;

        public InsertStatementReservationAction(IStatementsRepository statementsRepository)
        {
            _statementsRepository = statementsRepository;
        }

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var statementReservationDto = eventBody.ToObjectFromJson<StatementReservationEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<StatementReservationEventDTO, StatementReservation>()
                    .ForMember(dest => dest.Active, opt => opt.Ignore())
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var statementReservation = mapper.Map<StatementReservation>(statementReservationDto);

            return _statementsRepository.InsertStatementReservation(statementReservation, connection, transaction);
        }
    }
}