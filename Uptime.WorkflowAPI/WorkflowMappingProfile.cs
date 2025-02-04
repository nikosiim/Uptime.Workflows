using AutoMapper;
using Uptime.Application.Commands;
using Uptime.Application.DTOs;
using Uptime.Shared.Models.Documents;
using Uptime.Shared.Models.Libraries;
using Uptime.Shared.Models.Tasks;
using Uptime.Shared.Models.Workflows;
using Uptime.Shared.Models.WorkflowTemplates;

namespace Uptime.WorkflowAPI;

public class WorkflowMappingProfile : Profile
{
    public WorkflowMappingProfile()
    {
        CreateMap<DocumentWorkflowTaskDto, DocumentTasksResponse>();
        CreateMap<DocumentWorkflowDto, DocumentWorkflowsResponse>();
        CreateMap<WorkflowTaskDto, WorkflowTasksResponse>();
        CreateMap<LibraryDto, LibraryResponse>();
        CreateMap<LibraryDocumentDto, LibraryDocumentResponse>();
        CreateMap<LibraryWorkflowTemplateDto, LibraryWorkflowTemplateResponse>();
        CreateMap<WorkflowTemplateDto, WorkflowTemplateResponse>();
        CreateMap<WorkflowDto, WorkflowResponse>();
        CreateMap<WorkflowTemplateCreateRequest, CreateWorkflowTemplateCommand>();
        CreateMap<WorkflowTaskDto, WorkflowTaskResponse>();
    }
}