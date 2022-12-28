using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
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

        public DecisionProvider(
            ILogger<DecisionProvider> logger,
            IDecisionsReadOnlyRepository decisionsRepository)
        {
            _logger = logger;
            _decisionsRepository = decisionsRepository;
        }

        public async Task<WebApiDecisionDTO?> GetDecisision(string caseIdLabel, string language)
        {
            var decision = await _decisionsRepository.FetchDecisionsByCaseIdLabel(caseIdLabel, language);
            if (decision == null) 
            {
                return null;
            }

            return MapDecisionToDTO(decision);
        }

        private WebApiDecisionDTO MapDecisionToDTO(FullDecision fullDecision)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Decision, WebApiDecisionDTO>()
                    .ForMember(dest => dest.Attachments, opt => opt.MapFrom(_ => fullDecision.Attachments.Select(attachment => MapAttachmentToDTO(attachment)).ToList()))
                    .ForMember(dest => dest.Pdf, opt => opt.MapFrom(_ => MapAttachmentToDTO(fullDecision.Pdf)))
                    .ForMember(dest => dest.DecisionHistoryPdf, opt => opt.MapFrom(_ => MapAttachmentToDTO(fullDecision.DecisionHistoryPdf)));
            });
            var mapper = config.CreateMapper();
            var result = mapper.Map<WebApiDecisionDTO>(fullDecision.Decision);

            return result;
        }

        private WebApiAttachmentDTO MapAttachmentToDTO(DecisionAttachment attachment)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<DecisionAttachment, WebApiAttachmentDTO>();
            });
            var mapper = config.CreateMapper();
            var result = mapper.Map<WebApiAttachmentDTO>(attachment);

            return result;
        }

    }
}
