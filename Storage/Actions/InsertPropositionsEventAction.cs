using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    public class InsertPropositionsEventAction : IEventAction
    {
        public List<EventType> EventTypes { get; } = new()
            { EventType.Propositions };

        private readonly IPropositionsRepository _propositionsRepository;

        public InsertPropositionsEventAction(IPropositionsRepository propositionsRepository)
        {
            _propositionsRepository = propositionsRepository;
        }

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var propositionEventDto = eventBody.ToObjectFromJson<PropositionsEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PropositionDTO, Proposition>()
                    .ForMember(dest => dest.MeetingID, opt => opt.MapFrom(x => propositionEventDto.MeetingID))
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
            });
            var mapper = config.CreateMapper();
            var propositions = propositionEventDto.Propositions.Select(proposition => mapper.Map<Proposition>(proposition)).ToList();

            return _propositionsRepository.InsertPropositions(propositions, connection, transaction);
        }
    }
}