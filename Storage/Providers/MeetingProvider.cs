using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace Storage.Providers
{
    public interface IMeetingProvider
    {
        Task<MeetingDTO?> FetchById(string id);

        Task<MeetingDTO?> FetchNextUpcomingMeeting();
    }

    public class MeetingProvider : IMeetingProvider
    {
        private readonly IMeetingsRepository _meetingsRepository;
        private readonly IAgendaItemsRepository _agendaItemsRepository;

        public MeetingProvider(IMeetingsRepository meetingsRepository, IAgendaItemsRepository agendaItemsRepository)
        {
            _meetingsRepository = meetingsRepository;
            _agendaItemsRepository = agendaItemsRepository;
        }

        public async Task<MeetingDTO?> FetchById(string id)
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

        public async Task<MeetingDTO?> FetchNextUpcomingMeeting()
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

        private MeetingDTO MapMeetingToDTO(Meeting meeting)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Meeting, MeetingDTO>();
            });
            var mapper = config.CreateMapper();
            var meetingDTO = mapper.Map<MeetingDTO>(meeting);

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

    }
}
