using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Mappers;
using Storage.Providers.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace Storage.Providers
{
    public interface IDecisionProvider
    {
        Task<WebApiDecisionDTO?> GetDecisision(string caseIdLabel, string language);
    }

    public class DecisionProvider : IDecisionProvider
    {
        private readonly ILogger<DecisionProvider> _logger;
        private readonly IDecisionsReadOnlyRepository _decisionsRepository;
        private readonly IFullDecisionMapper _fullDecisionMapper;

        public DecisionProvider(
            ILogger<DecisionProvider> logger,
            IDecisionsReadOnlyRepository decisionsRepository,
            IFullDecisionMapper fullDecisionMapper)
        {
            _logger = logger;
            _decisionsRepository = decisionsRepository;
            _fullDecisionMapper = fullDecisionMapper;
        }

        public async Task<WebApiDecisionDTO?> GetDecisision(string caseIdLabel, string language)
        {
            var decision = await _decisionsRepository.FetchDecisionsByCaseIdLabel(caseIdLabel, language);
            if (decision == null) 
            {
                return null;
            }

            return _fullDecisionMapper.MapDecisionToDTO(decision);
        }
    }
}
