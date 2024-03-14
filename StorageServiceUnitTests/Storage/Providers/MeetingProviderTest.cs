using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Mappers;
using Storage.Providers;
using Storage.Providers.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Providers
{
    public class MeetingProviderTests
    {
        private readonly Mock<IMeetingsRepository> _meetingsRepository;
        private readonly Mock<IAgendaItemsRepository> _agendaItemsRepository;
        private readonly Mock<IDecisionsReadOnlyRepository> _decisionsRepository;
        private readonly Mock<IVideoSyncRepository> _videoSyncRepository;
        private readonly Mock<IFullDecisionMapper> _fullDecisionMapper;
        private readonly MeetingProvider _meetingsProvider;
        private readonly string id = "id";
        private readonly int agendaPoint = 2;
        private readonly string lang = "fi";
        private readonly string year = "2023";
        private readonly string sequenceNumber = "1";

        public MeetingProviderTests()
        {
            _decisionsRepository = new Mock<IDecisionsReadOnlyRepository>();
            _meetingsRepository = new Mock<IMeetingsRepository>();
            _videoSyncRepository = new Mock<IVideoSyncRepository>();
            _agendaItemsRepository = new Mock<IAgendaItemsRepository>();
            _fullDecisionMapper = new Mock<IFullDecisionMapper>();
            _meetingsProvider = new MeetingProvider(_meetingsRepository.Object,
                _agendaItemsRepository.Object,
                _decisionsRepository.Object,
                _videoSyncRepository.Object,
                _fullDecisionMapper.Object
            );
        }
        
        [Fact]
        public async void FetchAgendaSubItemsById_ReturnsExpectedData()
        {
            List<AgendaSubItem> agendaSubItemList = new()
            {
                new AgendaSubItem(),
                new AgendaSubItem()
            };
            _agendaItemsRepository.Setup(x => x.FetchAgendaSubItems(id, agendaPoint)).Returns(Task.FromResult(agendaSubItemList));
            var result = await _meetingsProvider.FetchAgendaSubItemsById(id, agendaPoint);

            _agendaItemsRepository.Verify(x => x.FetchAgendaSubItems(id, agendaPoint), Times.Once);
            Assert.NotNull(result);
            Assert.IsType<List<WebApiAgendaSubItemDTO>>(result);
        }

        [Fact]
        public async void FetchById_ReturnsExpectedData()
        {
            List<AgendaSubItem> agendaSubItemList = new()
            {
                new AgendaSubItem(),
                new AgendaSubItem()
            };

            Meeting meeting = new()
            {
                MeetingID = "meetingA",
                MeetingDate = DateTime.UtcNow,
                Name = "Test Meeting",
                MeetingSequenceNumber = 21,
                Location = "Testilä"
            };

            List<AgendaItem> agendaItems = new()
            {
                new AgendaItem(),
                new AgendaItem(),
            };

            List<AgendaItemAttachment> agendaItemAttachments = new()
            {
                new AgendaItemAttachment(),
                new AgendaItemAttachment(),
            };

            _meetingsRepository.Setup(x => x.FetchMeetingById(id)).Returns(Task.FromResult(meeting));
            _agendaItemsRepository.Setup(x => x.FetchAgendasByMeetingId(id, lang)).Returns(Task.FromResult(agendaItems));
            _agendaItemsRepository.Setup(x => x.FetchAgendaAttachmentsByMeetingId(meeting.MeetingID, lang)).Returns(Task.FromResult(agendaItemAttachments));
            var result = await _meetingsProvider.FetchById(id, lang);

            _agendaItemsRepository.Verify(x => x.FetchAgendasByMeetingId(id, lang), Times.Once);
            _agendaItemsRepository.Verify(x => x.FetchAgendaAttachmentsByMeetingId(meeting.MeetingID, lang), Times.Once);
            Assert.NotNull(result);
            Assert.IsType<WebApiMeetingDTO>(result);
        }

        [Fact]
        public async void FetchMeetingId_ReturnsExpectedData()
        {
            Meeting meeting = new()
            {
                MeetingID = "meetingA",
                MeetingDate = DateTime.UtcNow,
                Name = "Test Meeting",
                MeetingSequenceNumber = 21,
                Location = "Testilä"
            };

            _meetingsRepository.Setup(x => x.FetchMeetingByYearAndSeuquenceNumber(year, sequenceNumber)).Returns(Task.FromResult(meeting));
            var result = await _meetingsProvider.FetchMeetingId(year, sequenceNumber);

            _meetingsRepository.Verify(x => x.FetchMeetingByYearAndSeuquenceNumber(year, sequenceNumber), Times.Once);
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public async void FetchMeeting_ReturnsExpectedData()
        {
            Meeting meeting = new()
            {
                MeetingID = "meetingA",
                MeetingDate = DateTime.UtcNow,
                Name = "Test Meeting",
                MeetingSequenceNumber = 21,
                Location = "Testilä"
            };

            List<AgendaItem> agendaItems = new()
            {
                new AgendaItem(),
                new AgendaItem(),
                new AgendaItem(),
            };

            List<VideoSync> videoSyncs = new()
            {
                new VideoSync
                {
                    MeetingID = "meetingA",
                    Timestamp = DateTime.UtcNow,
                    VideoPosition = 34
                },
                new VideoSync
                {
                    MeetingID = "meetingA",
                    Timestamp = DateTime.UtcNow,
                    VideoPosition = 63
                },
            };

            List<AgendaItemAttachment> agendaItemAttachments = new()
            {
                new AgendaItemAttachment(),
                new AgendaItemAttachment(),
                new AgendaItemAttachment(),
            };

            List<FullDecision> decisions = new()
            {
                new FullDecision(),
                new FullDecision(),
                new FullDecision(),
            };

            _meetingsRepository.Setup(x => x.FetchMeetingByYearAndSeuquenceNumber(year, sequenceNumber)).Returns(Task.FromResult(meeting));
            _agendaItemsRepository.Setup(x => x.FetchAgendasByMeetingId(meeting.MeetingID, lang)).Returns(Task.FromResult(agendaItems));
            _agendaItemsRepository.Setup(x => x.FetchAgendaAttachmentsByMeetingId(meeting.MeetingID, lang)).Returns(Task.FromResult(agendaItemAttachments));
            _videoSyncRepository.Setup(x => x.GetVideoPositions(meeting.MeetingID)).Returns(Task.FromResult(videoSyncs));
            _decisionsRepository.Setup(x => x.FetchDecisionsByMeetingId(meeting.MeetingID, lang)).Returns(Task.FromResult(decisions));

            var result = await _meetingsProvider.FetchMeeting(year, sequenceNumber, lang);

            _meetingsRepository.Verify(x => x.FetchMeetingByYearAndSeuquenceNumber(year, sequenceNumber), Times.Once);
            _agendaItemsRepository.Verify(x => x.FetchAgendasByMeetingId(meeting.MeetingID, lang), Times.Once);
            _agendaItemsRepository.Verify(x => x.FetchAgendaAttachmentsByMeetingId(meeting.MeetingID, lang), Times.Once);
            _videoSyncRepository.Verify(x => x.GetVideoPositions(meeting.MeetingID), Times.Once);
            _decisionsRepository.Verify(x => x.FetchDecisionsByMeetingId(meeting.MeetingID, lang), Times.Once);
            Assert.NotNull(result);
            Assert.IsType<WebApiMeetingDTO>(result);
        }

        [Fact]
        public async void FetchNextUpcomingMeeting_ReturnsExpectedData()
        {
            Meeting meeting = new()
            {
                MeetingDate = DateTime.UtcNow.AddDays(1),
                MeetingID = "meetingA"
            };

            List<AgendaItem> agendaItems = new()
            {
                new AgendaItem(),
                new AgendaItem(),
                new AgendaItem(),
            };

            List<AgendaItemAttachment> agendaItemAttachments = new()
            {
                new AgendaItemAttachment(),
                new AgendaItemAttachment(),
                new AgendaItemAttachment(),
            };

            _meetingsRepository.Setup(x => x.FetchNextUpcomingMeeting()).Returns(Task.FromResult(meeting));
            _agendaItemsRepository.Setup(x => x.FetchAgendasByMeetingId(meeting.MeetingID, lang)).Returns(Task.FromResult(agendaItems));
            _agendaItemsRepository.Setup(x => x.FetchAgendaAttachmentsByMeetingId(meeting.MeetingID, lang)).Returns(Task.FromResult(agendaItemAttachments));

            var result = await _meetingsProvider.FetchNextUpcomingMeeting(lang);

            _meetingsRepository.Verify(x => x.FetchNextUpcomingMeeting(), Times.Once);
            _agendaItemsRepository.Verify(x => x.FetchAgendasByMeetingId(meeting.MeetingID, lang), Times.Once);
            _agendaItemsRepository.Verify(x => x.FetchAgendaAttachmentsByMeetingId(meeting.MeetingID, lang), Times.Once);

            Assert.NotNull(result);
            Assert.IsType<WebApiMeetingDTO>(result);
        }
    }
}