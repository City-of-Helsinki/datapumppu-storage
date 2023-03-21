using AutoMapper;
using Storage.Providers.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace Storage.Providers
{
    public interface IReservationsProvider
    {
        Task<List<WebApiReservationDTO>> GetReservations(string meetingId, string caseNumber);
    }

    public class ReservationsProvider : IReservationsProvider
    {
        private readonly ILogger<ReservationsProvider> _logger;
        private readonly IStatementsRepository _statementsRepository;

        public ReservationsProvider(
            ILogger<ReservationsProvider> logger,
            IStatementsRepository statementsRepository)
        {
            _logger = logger;
            _statementsRepository = statementsRepository;
        }

        public async Task<List<WebApiReservationDTO>> GetReservations(string meetingId, string caseNumber)
        {
            var statementReservations = await _statementsRepository.GetStatementReservations(meetingId, caseNumber);
            var replyReservations = await _statementsRepository.GetReplyReservations(meetingId, caseNumber);
            
            // add active speaker
            var activeSpeaker = await _statementsRepository.GetActiveSpeaker(meetingId, caseNumber);
            if(replyReservations != null && activeSpeaker != null)
            {
                replyReservations.Add(activeSpeaker);
            }
         
            if (statementReservations == null && replyReservations == null) 
            {
                return new List<WebApiReservationDTO>();
            }

            var result = MapReservationsToDTO(statementReservations, replyReservations);   

            return result;
        }

        private List<WebApiReservationDTO> MapReservationsToDTO(List<StatementReservation> statementReservations, List<ReplyReservation> replyReservations)
        {
            _logger.LogInformation("MapReservationsToDTO()");
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<StatementReservation, WebApiReservationDTO>()
                    .ForMember(dest => dest.AgendaPoint, opt => opt.MapFrom(src => src.CaseNumber));
                cfg.CreateMap<ReplyReservation, WebApiReservationDTO>()
                    .ForMember(dest => dest.AgendaPoint, opt => opt.MapFrom(src => src.CaseNumber));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var listA = statementReservations.Select(statementReservation => mapper.Map<WebApiReservationDTO>(statementReservation));
            var listB = replyReservations.Select(replyReservation => mapper.Map<WebApiReservationDTO>(replyReservation));
            var result = listA.Concat(listB);
            result = result.OrderBy(reservation => reservation.Ordinal);
            return result.ToList();
        }

    }
}
