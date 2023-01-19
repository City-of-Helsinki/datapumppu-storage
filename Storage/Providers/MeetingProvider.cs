using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Mappers;
using Storage.Providers.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace Storage.Providers
{
    public interface IMeetingProvider
    {
        Task<WebApiMeetingDTO?> FetchById(string id, string language);

        Task<WebApiMeetingDTO?> FetchNextUpcomingMeeting(string language);
        Task<WebApiMeetingDTO?> FetchMeeting(string year, string sequenceNumber, string language);
    }

    public class MeetingProvider : IMeetingProvider
    {
        private readonly IMeetingsRepository _meetingsRepository;
        private readonly IAgendaItemsRepository _agendaItemsRepository;
        private readonly IDecisionsReadOnlyRepository _decisionsRepository;
        private readonly IFullDecisionMapper _fullDecisionMapper;

        public MeetingProvider(IMeetingsRepository meetingsRepository,
            IAgendaItemsRepository agendaItemsRepository,
            IDecisionsReadOnlyRepository decisionsRepository,
            IFullDecisionMapper fullDecisionMapper)
        {
            _meetingsRepository = meetingsRepository;
            _agendaItemsRepository = agendaItemsRepository;
            _decisionsRepository = decisionsRepository;
            _fullDecisionMapper = fullDecisionMapper;
        }

        public async Task<WebApiMeetingDTO?> FetchById(string id, string language)
        {
            // fetch meeting by id
            var meeting = await _meetingsRepository.FetchMeetingById(id);
            if (meeting == null)
            {
                return null;
            }
            var agendaItems = await _agendaItemsRepository.FetchAgendasByMeetingId(id, language);
            var attachments = await _agendaItemsRepository.FetchAgendaAttachmentsByMeetingId(meeting.MeetingID, language);
            
            // map to DTO
            var agendaItemDTOs = MapAgendasToDTO(agendaItems, attachments);
            var meetingDTO = MapMeetingToDTO(meeting, agendaItemDTOs);
            meetingDTO.Agendas = agendaItemDTOs;

            return meetingDTO;
        }

        public async Task<WebApiMeetingDTO?> FetchMeeting(string year, string sequenceNumber, string language)
        {
            var meeting = await _meetingsRepository.FetchMeetingByYearAndSeuquenceNumber(year, sequenceNumber);
            if (meeting == null)
            {
                return null;
            }
            var agendaitems = await _agendaItemsRepository.FetchAgendasByMeetingId(meeting.MeetingID, language);

            var attachments = await _agendaItemsRepository.FetchAgendaAttachmentsByMeetingId(meeting.MeetingID, language);

            var decisions = await _decisionsRepository.FetchDecisionsByMeetingId(meeting.MeetingID, language);

            var agendaitemDTOs = MapAgendasToDTO(agendaitems, attachments);
            var meetingWebApiDTO = MapMeetingToDTO(meeting, agendaitemDTOs);
            
            var decisionDtos = decisions.Select(decision => _fullDecisionMapper.MapDecisionToDTO(decision)).ToList();

            meetingWebApiDTO.Agendas = agendaitemDTOs;
            meetingWebApiDTO.Decisions = decisionDtos;
            return meetingWebApiDTO;
        }

        public async Task<WebApiMeetingDTO?> FetchNextUpcomingMeeting(string language)
        {
            // fetch next upcoming meeting
            var meeting = await _meetingsRepository.FetchNextUpcomingMeeting();
            if (meeting == null)
            {
                return null;
            }
            string id = meeting.MeetingID;
            var agendaItems = await _agendaItemsRepository.FetchAgendasByMeetingId(id, language);
            var attachments = await _agendaItemsRepository.FetchAgendaAttachmentsByMeetingId(meeting.MeetingID, language);
            // map to DTO
            var agendaItemDTOs = MapAgendasToDTO(agendaItems, attachments);
            var meetingDTO = MapMeetingToDTO(meeting, agendaItemDTOs);
            
            meetingDTO.Agendas = agendaItemDTOs;

            return meetingDTO;
        }

        private WebApiMeetingDTO MapMeetingToDTO(Meeting meeting, List<WebApiAgendaItemDTO> agendaItems)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Meeting, WebApiMeetingDTO>()
                    .ForMember(dest => dest.Agendas, opt => opt.MapFrom(_ => agendaItems))
                    .ForMember(dest => dest.Decisions, opt => opt.Ignore());

            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var meetingDTO = mapper.Map<WebApiMeetingDTO>(meeting);

            return meetingDTO;
        }

        private List<WebApiAgendaItemDTO> MapAgendasToDTO(List<AgendaItem> agendaItems, List<AgendaItemAttachment> attachments)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AgendaItem, WebApiAgendaItemDTO>()
                    .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => attachments.Where(a => a.AgendaPoint == src.AgendaPoint)));
    
                cfg.CreateMap<AgendaItemAttachment, WebApiAttachmentDTO>();
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var result = agendaItems.Select(agenda => mapper.Map<WebApiAgendaItemDTO>(agenda)).ToList();

            return result;
        }
    }
}
