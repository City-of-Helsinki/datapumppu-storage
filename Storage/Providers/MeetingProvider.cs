using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Mappers;
using Storage.Providers.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using Storage.Repositories.Models.Extensions;

namespace Storage.Providers
{
    /// <summary>
    /// Provides business logic for retrieving and managing meeting information.
    /// Coordinates between multiple repositories to aggregate meeting, agenda, decision, and video sync data.
    /// </summary>
    public interface IMeetingProvider
    {
        /// <summary>
        /// Retrieves a meeting by its unique identifier.
        /// </summary>
        /// <param name="id">The unique meeting identifier (e.g., "029002023001").</param>
        /// <param name="language">The language code for localized content ("fi" for Finnish, "sv" for Swedish).</param>
        /// <returns>A WebApiMeetingDTO containing the meeting details, or null if not found.</returns>
        Task<WebApiMeetingDTO?> FetchById(string id, string language);

        /// <summary>
        /// Retrieves sub-items for a specific agenda point within a meeting.
        /// </summary>
        /// <param name="id">The unique meeting identifier.</param>
        /// <param name="agendaPoint">The agenda point number.</param>
        /// <returns>A list of WebApiAgendaSubItemDTO containing the agenda sub-items.</returns>
        Task<List<WebApiAgendaSubItemDTO>> FetchAgendaSubItemsById(string id, int agendaPoint);

        /// <summary>
        /// Retrieves the next upcoming meeting scheduled in the future.
        /// </summary>
        /// <param name="language">The language code for localized content.</param>
        /// <returns>A WebApiMeetingDTO containing the next upcoming meeting details, or null if no future meeting exists.</returns>
        Task<WebApiMeetingDTO?> FetchNextUpcomingMeeting(string language);
        
        /// <summary>
        /// Retrieves a meeting by year and sequence number.
        /// </summary>
        /// <param name="year">The year of the meeting (e.g., "2023").</param>
        /// <param name="sequenceNumber">The meeting sequence number within the year.</param>
        /// <param name="language">The language code for localized content.</param>
        /// <returns>A WebApiMeetingDTO containing the meeting details, or null if not found.</returns>
        Task<WebApiMeetingDTO?> FetchMeeting(string year, string sequenceNumber, string language);

        /// <summary>
        /// Retrieves the meeting ID for a given year and sequence number.
        /// </summary>
        /// <param name="year">The year of the meeting.</param>
        /// <param name="sequenceNumber">The meeting sequence number within the year.</param>
        /// <returns>The meeting ID as a string.</returns>
        Task<string> FetchMeetingId(string year, string sequenceNumber);
    }

    /// <summary>
    /// Implementation of IMeetingProvider that coordinates meeting data retrieval and video synchronization.
    /// Aggregates data from meetings, agenda items, decisions, and video sync repositories.
    /// </summary>
    public class MeetingProvider : IMeetingProvider
    {
        private readonly IMeetingsRepository _meetingsRepository;
        private readonly IAgendaItemsRepository _agendaItemsRepository;
        private readonly IDecisionsReadOnlyRepository _decisionsRepository;
        private readonly IFullDecisionMapper _fullDecisionMapper;
        private readonly IVideoSyncRepository _videoSyncRepository;

        /// <summary>
        /// Initializes a new instance of the MeetingProvider class.
        /// </summary>
        /// <param name="meetingsRepository">The repository for accessing meeting data.</param>
        /// <param name="agendaItemsRepository">The repository for accessing agenda item data.</param>
        /// <param name="decisionsRepository">The repository for accessing decision data.</param>
        /// <param name="videoSyncRepository">The repository for accessing video synchronization data.</param>
        /// <param name="fullDecisionMapper">The mapper for transforming decision entities to full DTOs.</param>
        public MeetingProvider(IMeetingsRepository meetingsRepository,
            IAgendaItemsRepository agendaItemsRepository,
            IDecisionsReadOnlyRepository decisionsRepository,
            IVideoSyncRepository videoSyncRepository,
            IFullDecisionMapper fullDecisionMapper)
        {
            _meetingsRepository = meetingsRepository;
            _agendaItemsRepository = agendaItemsRepository;
            _decisionsRepository = decisionsRepository;
            _fullDecisionMapper = fullDecisionMapper;
            _videoSyncRepository = videoSyncRepository;
        }

        public async Task<List<WebApiAgendaSubItemDTO>> FetchAgendaSubItemsById(string id, int agendaPoint)
        {
            return MapAgendasSubItemsToDTO(await _agendaItemsRepository.FetchAgendaSubItems(id, agendaPoint));
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

        public async Task<string> FetchMeetingId(string year, string sequenceNumber)
        {
            var meeting = await _meetingsRepository.FetchMeetingByYearAndSeuquenceNumber(year, sequenceNumber);
            return meeting?.MeetingID ?? string.Empty;
        }

        public async Task<WebApiMeetingDTO?> FetchMeeting(string year, string sequenceNumber, string language)
        {
            var meeting = await _meetingsRepository.FetchMeetingByYearAndSeuquenceNumber(year, sequenceNumber);
            if (meeting == null)
            {
                return null;
            }
            var agendaitems = await _agendaItemsRepository.FetchAgendasByMeetingId(meeting.MeetingID, language);

            agendaitems = await UpdateVideoPositions(meeting.MeetingID, agendaitems);

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

        private async Task<List<AgendaItem>> UpdateVideoPositions(string meetingId, List<AgendaItem> agendaItems)
        {
            var videoPositions = await _videoSyncRepository.GetVideoPositions(meetingId);
            foreach (var agendaItem in agendaItems)
            {
                agendaItem.VideoPosition = videoPositions.GetVideoPosition(agendaItem.Timestamp);
            }

            return agendaItems;
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

        private List<WebApiAgendaSubItemDTO> MapAgendasSubItemsToDTO(List<AgendaSubItem> agendaSubItems)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AgendaSubItem, WebApiAgendaSubItemDTO>();
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            return agendaSubItems.Select(item => mapper.Map<WebApiAgendaSubItemDTO>(item)).ToList();
        }

        private List<WebApiAgendaItemDTO> MapAgendasToDTO(
            List<AgendaItem> agendaItems,
            List<AgendaItemAttachment> attachments)
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
