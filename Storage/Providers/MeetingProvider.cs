using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
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
        private readonly IDecisionsRepository _decisionsRepository;

        public MeetingProvider(IMeetingsRepository meetingsRepository, IAgendaItemsRepository agendaItemsRepository, IDecisionsRepository decisionsRepository)
        {
            _meetingsRepository = meetingsRepository;
            _agendaItemsRepository = agendaItemsRepository;
            _decisionsRepository = decisionsRepository;
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
            // map to DTO
            var meetingDTO = MapMeetingToDTO(meeting);
            var agendaItemDTOs = MapAgendasToDTO(agendaItems);
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
            var decisions = await _decisionsRepository.FetchDecisionsByMeetingId(meeting.MeetingID, language);
            var meetingWebApiDTO = MapMeetingToDTO(meeting);
            var agendaitemDTOs = MapAgendasToDTO(agendaitems);
            var decisionDtos = decisions.Select(decision => MapDecisionToDTO(decision)).ToList();

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
            // map to DTO
            var meetingDTO = MapMeetingToDTO(meeting);
            var agendaItemDTOs = MapAgendasToDTO(agendaItems);
            meetingDTO.Agendas = agendaItemDTOs;

            return meetingDTO;
        }

        private WebApiMeetingDTO MapMeetingToDTO(Meeting meeting)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Meeting, WebApiMeetingDTO>();
            });
            var mapper = config.CreateMapper();
            var meetingDTO = mapper.Map<WebApiMeetingDTO>(meeting);

            return meetingDTO;
        }

        private List<WebApiAgendaItemDTO> MapAgendasToDTO(List<AgendaItem> agendaItems)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AgendaItem, WebApiAgendaItemDTO>();
            });
            var mapper = config.CreateMapper();
            var result = agendaItems.Select(agenda => mapper.Map<WebApiAgendaItemDTO>(agenda)).ToList();

            return result;
        }

        private WebApiDecisionDTO MapDecisionToDTO(FullDecision fullDecision)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Decision, WebApiDecisionDTO>()
                    .ForMember(dest => dest.Attachments, opt => opt.MapFrom(_ => fullDecision.Attachments.Select(attachment => MapAttachmentToDTO(attachment)).ToList()))
                    .ForMember(dest => dest.Pdf, opt => opt.MapFrom(_ => MapAttachmentToDTO(fullDecision.Pdf)))
                    .ForMember(dest => dest.DecisionHistoryPdf, opt => opt.MapFrom(_ => MapAttachmentToDTO(fullDecision.DecisionHistoryPdf)));
            });
            var mapper = config.CreateMapper();
            var result = mapper.Map<WebApiDecisionDTO>(fullDecision.Decision);

            return result;
        }

        private WebApiAttachmentDTO MapAttachmentToDTO(DecisionAttachment attachment)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<DecisionAttachment, WebApiAttachmentDTO>();
            });
            var mapper = config.CreateMapper();
            var result = mapper.Map<WebApiAttachmentDTO>(attachment);

            return result;
        }

    }
}
