using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Mappers;
using Storage.Providers.DTOs;
using Storage.Repositories;
using Storage.Repositories.Models;

namespace Storage.Providers
{
    /// <summary>
    /// Provides business logic for retrieving decision information.
    /// Coordinates between the decisions repository and decision mapper to produce API-ready DTOs.
    /// </summary>
    public interface IDecisionProvider
    {
        /// <summary>
        /// Retrieves a decision by its case ID label and language.
        /// </summary>
        /// <param name="caseIdLabel">The case identifier label (e.g., "029-2023-1234").</param>
        /// <param name="language">The language code for localized content ("fi" for Finnish, "sv" for Swedish).</param>
        /// <returns>A WebApiDecisionDTO containing the decision details, or null if not found.</returns>
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
