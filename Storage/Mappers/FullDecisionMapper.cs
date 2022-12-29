using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Providers.DTOs;
using Storage.Repositories.Models;

namespace Storage.Mappers
{
    public interface IFullDecisionMapper
    {
        WebApiDecisionDTO MapDecisionToDTO(FullDecision fullDecision);
    }

    public class FullDecisionMapper : IFullDecisionMapper
    { 
        public WebApiDecisionDTO MapDecisionToDTO(FullDecision fullDecision)
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

        private WebApiAttachmentDTO? MapAttachmentToDTO(DecisionAttachment? attachment)
        {
            if (attachment == null)
            {
                return null;
            }

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
