using Microsoft.AspNetCore.Mvc;
using Storage.Providers;
using Storage.Repositories;

namespace Storage.Controllers
{
    [ApiController]
    [Route("api/decisions/")]
    public class DecisionsController : ControllerBase
    {
        private readonly ILogger<DecisionsController> _logger;
        private readonly IDecisionProvider _decisionProvider;

        public DecisionsController(ILogger<DecisionsController> logger,
            IDecisionProvider decisionProvider)
        {
            _logger = logger;
            _decisionProvider = decisionProvider;
        }

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
