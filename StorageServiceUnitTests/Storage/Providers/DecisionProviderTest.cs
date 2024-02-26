using Microsoft.Extensions.Logging;
using Storage.Mappers;
using Storage.Providers;
using Storage.Providers.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace StorageServiceUnitTests.Storage.Providers
{
    public class DecisionProviderTests
    {
        private readonly Mock<IDecisionsReadOnlyRepository> _decisionsRepository;
        private readonly Mock<ILogger<DecisionProvider>> _logger;
        private readonly Mock<IFullDecisionMapper> _fullDecisionMapper;
        private readonly DecisionProvider _decisionProvider;
        private readonly string caseLabel = "caseLabel";
        private readonly string lang = "fi";
        private readonly Decision decision = new()
        {
            MeetingID = "meetingA",
            NativeId = "nativeId",
        };

        public DecisionProviderTests()
        {
            _decisionsRepository = new Mock<IDecisionsReadOnlyRepository>();
            _logger = new Mock<ILogger<DecisionProvider>>();
            _fullDecisionMapper = new Mock<IFullDecisionMapper>();
            _decisionProvider = new DecisionProvider(_logger.Object, _decisionsRepository.Object, _fullDecisionMapper.Object);
        }
        [Fact]
        public async void GetDecision_ReturnsExpectedData()
        {
            FullDecision fullDecision = new()
            {
                Decision = decision,
                Pdf = new DecisionAttachment(),
                DecisionHistoryPdf = new DecisionAttachment()
            };

            WebApiDecisionDTO webApiDecisionDto = new();

            _decisionsRepository.Setup(x => x.FetchDecisionsByCaseIdLabel(caseLabel, lang)).Returns(Task.FromResult(fullDecision));
            _fullDecisionMapper.Setup(x => x.MapDecisionToDTO(fullDecision)).Returns(webApiDecisionDto);

            var result = await _decisionProvider.GetDecisision(caseLabel, lang);

            _decisionsRepository.Verify(x => x.FetchDecisionsByCaseIdLabel(caseLabel, lang), Times.Once);
            Assert.NotNull(result);
            Assert.IsType<WebApiDecisionDTO>(result);
        }
    }
}