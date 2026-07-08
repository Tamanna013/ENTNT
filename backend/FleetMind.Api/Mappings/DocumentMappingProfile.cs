using System.Linq;
using AutoMapper;
using FleetMind.Api.Models;
using FleetMind.Api.DTOs.Documents;

namespace FleetMind.Api.Mappings;

public class DocumentMappingProfile : Profile
{
    public DocumentMappingProfile()
    {
        CreateMap<Document, DocumentDto>()
            .ForMember(dest => dest.CurrentVersionDownloadUrl, opt => opt.MapFrom(src =>
                src.Versions.FirstOrDefault(v => v.VersionNumber == src.CurrentVersionNumber) != null
                    ? $"/api/v1/attachments/{src.Versions.FirstOrDefault(v => v.VersionNumber == src.CurrentVersionNumber)!.AttachmentId}/download"
                    : string.Empty));

        CreateMap<DocumentVersion, DocumentVersionDto>()
            .ForMember(dest => dest.DownloadUrl, opt => opt.MapFrom(src => $"/api/v1/attachments/{src.AttachmentId}/download"))
            .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.Attachment.FileName))
            .ForMember(dest => dest.UploadedByUserName, opt => opt.MapFrom(src => src.UploadedByUser.FirstName + " " + src.UploadedByUser.LastName));
    }
}
