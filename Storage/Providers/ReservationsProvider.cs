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
        private readonly IEventsRepository _eventsRepository;

        public ReservationsProvider(
            ILogger<ReservationsProvider> logger,
            IStatementsRepository statementsRepository,
            IEventsRepository eventsRepository)
        {
            _logger = logger;
            _statementsRepository = statementsRepository;
            _eventsRepository = eventsRepository;
        }

        public async Task<List<WebApiReservationDTO>> GetReservations(string meetingId, string caseNumber)
        {
            if (!await _eventsRepository.IsAgendaPointHandled(meetingId, caseNumber))
            {
                return new List<WebApiReservationDTO>();
            }

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

            // there could be duplicates if reservation list is not yet cleared and there is someone already with an open microphone
            result = RemoveDuplicates(result);

            return result;
        }

        private List<WebApiReservationDTO> RemoveDuplicates(List<WebApiReservationDTO> reservations)
        {
            if (reservations.Count > 1 && reservations[0].Person == reservations[1].Person)
            {
                if (reservations[0].Active == true)
                {
                    reservations.RemoveAt(1);
                }
                else if (reservations[1].Active == true)
                {
                    reservations.RemoveAt(0);
                }
            }

            return reservations;
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
