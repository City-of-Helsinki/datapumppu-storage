using AutoMapper;
using Storage.Controllers.MeetingInfo.DTOs;
using Storage.Providers.DTOs;
using Storage.Repositories.Models;

namespace Storage.Mappers
{
    /// <summary>
    /// Defines the contract for mapping decision entities to data transfer objects.
    /// </summary>
    public interface IFullDecisionMapper
    {
        /// <summary>
        /// Maps a full decision entity (including attachments and PDFs) to a web API DTO.
        /// </summary>
        /// <param name="fullDecision">The complete decision entity with all related data.</param>
        /// <returns>The mapped web API decision DTO.</returns>
        WebApiDecisionDTO MapDecisionToDTO(FullDecision fullDecision);
    }

    /// <summary>
    /// Maps decision entities and their related attachments to web API data transfer objects.
    /// Uses AutoMapper to configure mapping rules for complex decision structures.
    /// </summary>
    public class FullDecisionMapper : IFullDecisionMapper
    { 
        /// <summary>
        /// Maps a full decision entity to a web API DTO.
        /// Includes mapping for decision attachments, PDF documents, and decision history PDFs.
        /// </summary>
        /// <param name="fullDecision">The complete decision entity with attachments and PDFs.</param>
        /// <returns>The mapped web API decision DTO with all related data.</returns>
        public WebApiDecisionDTO MapDecisionToDTO(FullDecision fullDecision)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Decision, WebApiDecisionDTO>()
                    .ForMember(dest => dest.Attachments, opt => opt.MapFrom(_ => fullDecision.Attachments.Select(attachment => MapAttachmentToDTO(attachment)).ToList()))
                    .ForMember(dest => dest.Pdf, opt => opt.MapFrom(_ => MapAttachmentToDTO(fullDecision.Pdf)))
                    .ForMember(dest => dest.DecisionHistoryPdf, opt => opt.MapFrom(_ => MapAttachmentToDTO(fullDecision.DecisionHistoryPdf)));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var result = mapper.Map<WebApiDecisionDTO>(fullDecision.Decision);

            return result;
        }

        /// <summary>
        /// Maps a decision attachment entity to a web API attachment DTO.
        /// Returns null if the attachment is null.
        /// </summary>
        /// <param name="attachment">The decision attachment entity to map, or null.</param>
        /// <returns>The mapped web API attachment DTO, or null if the input was null.</returns>
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
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var result = mapper.Map<WebApiAttachmentDTO>(attachment);

            return result;
        }

    }
}
