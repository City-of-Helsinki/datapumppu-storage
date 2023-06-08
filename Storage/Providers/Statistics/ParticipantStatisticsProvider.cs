using AutoMapper;
using Storage.Providers.Statistics.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models.Statistics;

namespace Storage.Providers.Statistics
{
    public interface IParticipantStatisticsProvider
    {
        Task<List<ParticipationsPersonDTO>> GetStatistics(int year);
    }

    public class ParticipantStatisticsProvider : IParticipantStatisticsProvider
    {
        private readonly IParticipantsRepository _participantsRepository;
        private readonly IAgendaItemsRepository _agendaItemsRepository;
        private readonly IMeetingSeatsRepository _meetingSeatsRepository;
        private readonly ILogger<ParticipantStatisticsProvider> _logger;


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

            participations.Sort((s1, s2) => s1.Person.CompareTo(s2));

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
