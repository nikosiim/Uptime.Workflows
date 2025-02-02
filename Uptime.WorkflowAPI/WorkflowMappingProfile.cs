using AutoMapper;
using Uptime.Application.Commands;
using Uptime.Application.DTOs;
using Uptime.Shared.Models.Documents;
using Uptime.Shared.Models.Libraries;
using Uptime.Shared.Models.Workflows;
using Uptime.Shared.Models.WorkflowTemplates;

namespace Uptime.WorkflowAPI;

public class WorkflowMappingProfile : Profile
{
    public WorkflowMappingProfile()
    {
        CreateMap<DocumentWorkflowTaskDto, DocumentTasksResponse>()
            .ForMember(dest => dest.Status, 
                opt => opt.MapFrom(src => src.Status));

        CreateMap<DocumentWorkflowDto, DocumentWorkflowsResponse>()
            .ForMember(dest => dest.Status, 
                opt => opt.MapFrom(src => src.Status));

        CreateMap<WorkflowTaskDto, WorkflowTasksResponse>()
            .ForMember(dest => dest.Status, 
                opt => opt.MapFrom(src => src.Status));

        CreateMap<LibraryDto, LibraryResponse>();
        CreateMap<LibraryDocumentDto, LibraryDocumentResponse>();
        CreateMap<LibraryWorkflowTemplateDto, LibraryWorkflowTemplateResponse>();
        CreateMap<WorkflowTemplateDto, WorkflowTemplateResponse>();

        CreateMap<WorkflowTemplateCreateRequest, CreateWorkflowTemplateCommand>();
    }
}