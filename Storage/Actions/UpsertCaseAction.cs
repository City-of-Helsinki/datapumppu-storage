using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    public class UpsertCaseAction : IEventAction
    {
        public List<EventType> EventTypes { get; } = new()
            { EventType.Case };

        private readonly ICaseRepository _caseRepository;

        public UpsertCaseAction(ICaseRepository caseRepository)
        {
            _caseRepository = caseRepository;
        }

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var caseEventDto = eventBody.ToObjectFromJson<CaseEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CaseEventDTO, Case>()
                    .ForMember(dest => dest.MeetingID, opt => opt.MapFrom(x => caseEventDto.MeetingID))
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var caseItem = mapper.Map<Case>(caseEventDto);

            return _caseRepository.UpsertCase(caseItem, connection, transaction);
        }
    }
}
