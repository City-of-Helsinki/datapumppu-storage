using Microsoft.AspNetCore.Mvc;
using Storage.Providers;
using Storage.Repositories;

namespace Storage.Controllers
{
    /// <summary>
    /// Provides API endpoints for retrieving meeting decisions.
    /// </summary>
    [ApiController]
    [Route("api/decisions/")]
    public class DecisionsController : ControllerBase
    {
        private readonly ILogger<DecisionsController> _logger;
        private readonly IDecisionProvider _decisionProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecisionsController"/> class.
        /// </summary>
        /// <param name="logger">Logger for recording controller operations.</param>
        /// <param name="decisionProvider">Provider for decision data operations.</param>
        public DecisionsController(ILogger<DecisionsController> logger,
            IDecisionProvider decisionProvider)
        {
            _logger = logger;
            _decisionProvider = decisionProvider;
        }

        /// <summary>
        /// Retrieves a decision by its case ID label in the specified language.
        /// </summary>
        /// <param name="caseIdLabel">The case ID label identifying the decision.</param>
        /// <param name="language">The language code (e.g., 'fi' for Finnish, 'sv' for Swedish) for localized content.</param>
        /// <returns>Returns 200 OK with the decision data, or 500 Internal Server Error if the operation fails.</returns>
        [HttpGet("{caseIdLabel}/{language}")]
        public async Task<IActionResult> GetDecisions(string caseIdLabel, string language)
        {
            try
            {
                _logger.LogInformation($"GetDecisions {caseIdLabel}, {language}");
                var decision = await _decisionProvider.GetDecisision(caseIdLabel, language);
                _logger.LogInformation($"found items: {decision?.CaseIDLabel}");
                return new OkObjectResult(decision);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetDecisions failed");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
