using AutoMapper;
using Storage.Events.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using System.Data;

namespace Storage.Actions
{
    public class UpdateVotingStatusAction : IEventAction
    {
        public List<EventType> EventTypes { get; } = new()
            { EventType.VotingStarted, EventType.VotingEnded };

        private readonly IVotingsRepository _votingsRepository;

        public UpdateVotingStatusAction(IVotingsRepository votingsRepository)
        {
            _votingsRepository = votingsRepository;
        }

        public Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction)
        {
            var votingEventDto = eventBody.ToObjectFromJson<VotingEventDTO>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VotingEventDTO, VotingEvent>()
                    .ForMember(dest => dest.EventID, opt => opt.MapFrom(x => eventId));
                cfg.CreateMap<VoteDTO, Vote>()
                    .ForMember(dest => dest.VotingID, opt => opt.MapFrom(x => votingEventDto.VotingID));
            });
            var mapper = config.CreateMapper();
            var votingEvent = mapper.Map<VotingEvent>(votingEventDto);

            if (votingEventDto.EventType == EventType.VotingStarted)
            {
                return _votingsRepository.InsertVoting(votingEvent, connection, transaction);
            }
     
            return _votingsRepository.SaveVotingResult(votingEvent, connection, transaction);
        }
    }
}
