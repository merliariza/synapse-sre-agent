using AutoMapper;
using SynapseSRE.Domain.Entities;
using SynapseSRE.Application.DTOs; 

namespace SynapseSRE.Api.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<User, UserDto>();

        CreateMap<Incident, IncidentDto>().ReverseMap();

        CreateMap<IncidentAttachment, IncidentAttachmentDto>().ReverseMap();

        CreateMap<TriageAnalysis, TriageAnalysisDto>().ReverseMap();

        CreateMap<ActivityLog, ActivityLogDto>();
    }
}