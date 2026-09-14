using AutoMapper;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models.Statistics;

namespace Storage.Providers.Statistics
{
    /// <summary>
    /// Provides business logic for retrieving participant attendance statistics by year.
    /// Aggregates meeting attendance data with agenda point participation details.
    /// </summary>
    public interface IParticipantStatisticsProvider
    {
        /// <summary>
        /// Retrieves attendance statistics for all participants within a given year.
        /// Returns meeting attendance and agenda point participation organized by person.
        /// </summary>
        /// <param name="year">The year to retrieve statistics for.</param>
        /// <returns>A list of ParticipationsPersonDTO containing attendance and participation details by person.</returns>
        Task<List<ParticipationsPersonDTO>> GetStatistics(int year);
    }

    /// <summary>
    /// Implementation of IParticipantStatisticsProvider that aggregates participant attendance data.
    /// Coordinates between participants, agenda items, and meeting seats repositories to build comprehensive participation statistics.
    /// </summary>
    public class ParticipantStatisticsProvider : IParticipantStatisticsProvider
    {
        private readonly IParticipantsRepository _participantsRepository;
        private readonly IAgendaItemsRepository _agendaItemsRepository;
        private readonly IMeetingSeatsRepository _meetingSeatsRepository;
        private readonly ILogger<ParticipantStatisticsProvider> _logger;


        /// <summary>
        /// Initializes a new instance of the ParticipantStatisticsProvider class.
        /// </summary>
        /// <param name="logger">The logger for diagnostic information.</param>
        /// <param name="participantsRepository">The repository for accessing participant data.</param>
        /// <param name="agendaItemsRepository">The repository for accessing agenda item data.</param>
        /// <param name="meetingSeatsRepository">The repository for accessing meeting seat data.</param>
        public ParticipantStatisticsProvider(
            ILogger<ParticipantStatisticsProvider> logger,
            IParticipantsRepository participantsRepository,
            IAgendaItemsRepository agendaItemsRepository,
            IMeetingSeatsRepository meetingSeatsRepository)
        {
            _logger = logger;
            _participantsRepository = participantsRepository;
            _agendaItemsRepository = agendaItemsRepository;
            _meetingSeatsRepository = meetingSeatsRepository;
        }

        public async Task<List<ParticipationsPersonDTO>> GetStatistics(int year)
        {
            _logger.LogInformation("GetStatistics {0}", year);

            var persons = await _participantsRepository.GetParticipants(year);
            var agendas = await _agendaItemsRepository.FetchAgendasByYear(year);


            var participations = new List<ParticipationsPersonDTO>();

            foreach (var agenda in agendas)
            {
                var agendaPointSeats = (await _meetingSeatsRepository.GetSeats(agenda.MeetingID, agenda.AgendaPoint.ToString()))
                    .Where(s => !string.IsNullOrEmpty(s.Person));

                foreach (var seat in agendaPointSeats)
                {
                    var participant = FindPersonParticipations(participations, seat.Person);
                    var meeting = FindParticipationMeeting(participant, agenda.MeetingID);                    
                    meeting.AgendaPoint.Add(agenda.AgendaPoint);
                    meeting.AgendaPoint.Sort();
                    meeting.AgendaPoint = meeting.AgendaPoint.Distinct().ToList();
                    _logger.LogInformation("Agenda point seats {0}/{1}/{2}", seat.Person, agenda.MeetingID, agenda.AgendaPoint);
                }
            }

            participations.Sort((s1, s2) => s1.Person.CompareTo(s2.Person));

            return participations ?? new List<ParticipationsPersonDTO>();
        }

        private ParticipationsPersonDTO FindPersonParticipations(List<ParticipationsPersonDTO> participations, string person)
        {
            var participant = participations.FirstOrDefault(p => p.Person == person);
            if (participant == null)
            {
                participant = new ParticipationsPersonDTO
                {
                    Person = person,
                };
                participations.Add(participant);
            }

            return participant;
        }

        private ParticipationsMeetingDTO FindParticipationMeeting(ParticipationsPersonDTO participation, string meetingId)
        {
            var meeting = participation.Meetings.FirstOrDefault(m => m.MeetingId == meetingId);
            if (meeting == null)
            {
                meeting = new ParticipationsMeetingDTO
                {
                    MeetingId = meetingId,
                    AgendaPoint = new List<int>()
                };
                participation.Meetings.Add(meeting);
            }

            return meeting;
        }

        private VotingStatisticsDTO MapSeatsToDTO(VotingStatistics stats)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VotingStatistics, VotingStatisticsDTO>();
            });
            config.AssertConfigurationIsValid();

            return config.CreateMapper().Map<VotingStatisticsDTO>(stats);
        }
    }
}
