using System.Data;
using Microsoft.Extensions.Logging;
using Storage.Actions;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;
using Storage.Repositories.Providers;

namespace StorageServiceUnitTests.Storage.Actions
{
    public class UpsertMeetingActionTest
    {
        [Fact]
        public async Task ExecuteShouldCallAllRequiredMethods()
        {
            var mockedConnection = new Mock<IDbConnection>();
            var mockedTransaction = new Mock<IDbTransaction>();
            var mockedConnectionFactory = new Mock<IDatabaseConnectionFactory>();

            mockedConnectionFactory.Setup(x => x.CreateOpenConnection()).Returns(Task.FromResult(mockedConnection.Object));
            mockedConnection.Setup(x => x.BeginTransaction()).Returns(mockedTransaction.Object);

            var meetingsRepository = new Mock<IMeetingsRepository>();
            var agendaItemsRepository = new Mock<IAgendaItemsRepository>();
            var decisionsRepository = new Mock<IDecisionsRepository>();
            var logger = new Mock<ILogger<UpsertMeetingAction>>();
            var upsertMeetingAction = new UpsertMeetingAction(mockedConnectionFactory.Object,
                meetingsRepository.Object,
                agendaItemsRepository.Object,
                decisionsRepository.Object,
                logger.Object);

            List<AgendaItemDTO> agendaItemDtos = new()
            {
                new AgendaItemDTO(),
                new AgendaItemDTO(),
                new AgendaItemDTO(),
            };

            List<AttachmentDTO> attatchmentDtoList = new()
            {
                new AttachmentDTO(),
                new AttachmentDTO(),
            };

            List<DecisionDTO> decisionList = new()
            {
                new()
                {
                    Pdf = new AttachmentDTO(),
                    Attachments = attatchmentDtoList
                },
            };

            MeetingDTO meetingDto = new()
            {
                MeetingID = "meetingA",
                Agendas = agendaItemDtos,
                Decisions = decisionList
            };

            await upsertMeetingAction.Execute(meetingDto);

            meetingsRepository.Verify(x => x.UpsertMeeting(
                It.IsAny<Meeting>(), mockedConnection.Object, mockedTransaction.Object),
                Times.Once);
            agendaItemsRepository.Verify(x => x.UpsertAgendaItems(
                It.IsAny<List<AgendaItem>>(), mockedConnection.Object, mockedTransaction.Object),
                Times.Once);
            agendaItemsRepository.Verify(x => x.UpsertAgendaItemPdfs(
                It.IsAny<List<AgendaItemAttachment>>(), mockedConnection.Object, mockedTransaction.Object),
                Times.Once);
            agendaItemsRepository.Verify(x => x.UpsertAgendaItemDecisionHistoryPdfs(
                It.IsAny<List<AgendaItemAttachment>>(), mockedConnection.Object, mockedTransaction.Object),
                Times.Once);
            decisionsRepository.Verify(x => x.UpsertDecisions(
                It.IsAny<List<Decision>>(), mockedConnection.Object, mockedTransaction.Object),
                Times.Once);
            decisionsRepository.Verify(x => x.UpsertDecisionAttachments(
                It.IsAny<List<DecisionAttachment>>(), mockedConnection.Object, mockedTransaction.Object),
                Times.Once);
            decisionsRepository.Verify(x => x.UpsertDecisionPdfs(
                It.IsAny<List<DecisionAttachment>>(), mockedConnection.Object, mockedTransaction.Object),
                Times.Once);
            decisionsRepository.Verify(x => x.UpsertDecisionHistoryPdfs(
                It.IsAny<List<DecisionAttachment>>(), mockedConnection.Object, mockedTransaction.Object),
                Times.Once);
        }
    }
}