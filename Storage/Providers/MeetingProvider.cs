using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Providers.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace Storage.Providers
{
    public interface IMeetingProvider
    {
        Task<MeetingWebApiDTO?> FetchById(string id);

        Task<MeetingWebApiDTO?> FetchNextUpcomingMeeting();
        Task<MeetingWebApiDTO?> FetchMeeting(string year, string sequenceNumber);
    }

    public class MeetingProvider : IMeetingProvider
    {
        private readonly IMeetingsRepository _meetingsRepository;
        private readonly IAgendaItemsRepository _agendaItemsRepository;
        private readonly IDecisionsRepository _decisionsRepository;

        public MeetingProvider(IMeetingsRepository meetingsRepository, IAgendaItemsRepository agendaItemsRepository, IDecisionsRepository decisionsRepository)
        {
            _meetingsRepository = meetingsRepository;
            _agendaItemsRepository = agendaItemsRepository;
            _decisionsRepository = decisionsRepository;
        }

        public async Task<MeetingWebApiDTO?> FetchById(string id)
        {
            // fetch meeting by id
            var meeting = await _meetingsRepository.FetchMeetingById(id);
            if (meeting == null)
            {
                return null;
            }
            var agendaItems = await _agendaItemsRepository.FetchAgendasByMeetingId(id);
            // map to DTO
            var meetingDTO = MapMeetingToDTO(meeting);
            var agendaItemDTOs = MapAgendasToDTO(agendaItems);
            meetingDTO.Agendas = agendaItemDTOs;

            return meetingDTO;
        }

        public async Task<MeetingWebApiDTO?> FetchMeeting(string year, string sequenceNumber)
        {
            var meeting = await _meetingsRepository.FetchMeetingByYearAndSeuquenceNumber(year, sequenceNumber);
            if (meeting == null)
            {
                return null;
            }
            var agendaitems = await _agendaItemsRepository.FetchAgendasByMeetingId(meeting.MeetingID);
            var decisions = await _decisionsRepository.FetchDecisionsByMeetingId(meeting.MeetingID);
            var meetingWebApiDTO = MapMeetingToDTO(meeting);
            var agendaitemDTOs = MapAgendasToDTO(agendaitems);
            var decisionDtos = decisions.Select(decision => MapDecisionToDTO(decision)).ToList();

            meetingWebApiDTO.Agendas = agendaitemDTOs;
            meetingWebApiDTO.Decisions = decisionDtos;
            return meetingWebApiDTO;
        }

        public async Task<MeetingWebApiDTO?> FetchNextUpcomingMeeting()
        {
            // fetch next upcoming meeting
            var meeting = await _meetingsRepository.FetchNextUpcomingMeeting();
            if (meeting == null)
            {
                return null;
            }
            string id = meeting.MeetingID;
            var agendaItems = await _agendaItemsRepository.FetchAgendasByMeetingId(id);
            // map to DTO
            var meetingDTO = MapMeetingToDTO(meeting);
            var agendaItemDTOs = MapAgendasToDTO(agendaItems);
            meetingDTO.Agendas = agendaItemDTOs;

            return meetingDTO;
        }

        private MeetingWebApiDTO MapMeetingToDTO(Meeting meeting)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Meeting, MeetingWebApiDTO>();
            });
            var mapper = config.CreateMapper();
            var meetingDTO = mapper.Map<MeetingWebApiDTO>(meeting);

            return meetingDTO;
        }

        private List<AgendaItemDTO> MapAgendasToDTO(List<AgendaItem> agendaItems)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AgendaItem, AgendaItemDTO>();
            });
            var mapper = config.CreateMapper();
            var result = agendaItems.Select(agenda => mapper.Map<AgendaItemDTO>(agenda)).ToList();

            return result;
        }

        private DecisionWebApiDTO MapDecisionToDTO(FullDecision fullDecision)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Decision, DecisionWebApiDTO>()
                    .ForMember(dest => dest.Attachments, opt => opt.MapFrom(_ => fullDecision.Attachments))
                    .ForMember(dest => dest.Pdf, opt => opt.MapFrom(_ => fullDecision.Pdf))
                    .ForMember(dest => dest.DecisionHistoryPdf, opt => opt.MapFrom(_ => fullDecision.DecisionHistoryPdf));
            });
            var mapper = config.CreateMapper();
            var result = mapper.Map<DecisionWebApiDTO>(fullDecision.Decision);

            return result;
        }

    }
}
