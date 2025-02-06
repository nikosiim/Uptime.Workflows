using Uptime.Application.Commands;
using Uptime.Application.DTOs;
using Uptime.Shared.Models.Documents;
using Uptime.Shared.Models.Libraries;
using Uptime.Shared.Models.Tasks;
using Uptime.Shared.Models.Workflows;
using Uptime.Shared.Models.WorkflowTemplates;

namespace Uptime.WorkflowAPI;

public static class Mapper
{
    #region Documents

    public static List<DocumentWorkflowsResponse> MapToDocumentWorkflowsResponse(List<DocumentWorkflowDto> source)
    {
        return source.Select(dto => new DocumentWorkflowsResponse
        {
            Id = dto.Id,
            TemplateId = dto.TemplateId,
            WorkflowTemplateName = dto.WorkflowTemplateName,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = dto.Status
        }).ToList();
    }

    public static List<DocumentTasksResponse> MapToDocumentTasksResponse(List<DocumentWorkflowTaskDto> source)
    {
        return source.Select(dto => new DocumentTasksResponse
        {
            TaskId = dto.TaskId,
            WorkflowId = dto.WorkflowId,
            AssignedTo = dto.AssignedTo,
            Status = dto.Status,
            TaskDescription = dto.TaskDescription,
            DueDate = dto.DueDate,
            EndDate = dto.EndDate
        }).ToList();
    }

    #endregion

    #region Libraries

    public static List<LibraryDocumentResponse> MapToLibraryDocumentResponse(List<LibraryDocumentDto> source)
    {
        return source.Select(dto => new LibraryDocumentResponse
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            LibraryId = dto.LibraryId
        }).ToList();
    }

    public static List<LibraryWorkflowTemplateResponse> MapToLibraryWorkflowTemplateResponse(List<LibraryWorkflowTemplateDto> source)
    {
        return source.Select(dto => new LibraryWorkflowTemplateResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            WorkflowBaseId = dto.WorkflowBaseId,
            AssociationDataJson = dto.AssociationDataJson,
            Created = dto.Created
        }).ToList();
    }

    #endregion

    #region Workflows
    
    public static WorkflowResponse MapToWorkflowResponse(WorkflowDto source)
    {
        return new WorkflowResponse
        {
            Status = source.Status,
            Originator = source.Originator,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            DocumentId = source.DocumentId,
            Document = source.Document
        };
    }

    public static List<WorkflowTasksResponse> MapToWorkflowTasksResponse(List<WorkflowTaskDto> source)
    {
        return source.Select(dto => new WorkflowTasksResponse
        {
            Id = dto.Id,
            AssignedTo = dto.AssignedTo,
            AssignedBy = dto.AssignedBy,
            Status = dto.Status,
            Description = dto.Description,
            DueDate = dto.DueDate ?? DateTime.UtcNow, // Default to current UTC time if null
            EndDate = dto.EndDate,
            StorageJson = dto.StorageJson
        }).ToList();
    }

    public static StartWorkflowCommand MapToStartWorkflowCommand(StartWorkflowRequest source)
    {
        return new StartWorkflowCommand
        {
            Originator = source.Originator,
            DocumentId = source.DocumentId,
            WorkflowTemplateId = source.WorkflowTemplateId,
            Data = source.Data
        };
    }

    #endregion

    #region WorkflowTasks

    public static WorkflowTaskResponse MapToWorkflowTaskResponse(WorkflowTaskDto source)
    {
        return new WorkflowTaskResponse
        {
            Id = source.Id,
            AssignedTo = source.AssignedTo,
            AssignedBy = source.AssignedBy,
            Status = source.Status,
            Description = source.Description,
            DueDate = source.DueDate ?? DateTime.UtcNow, // Default to current UTC time if null
            EndDate = source.EndDate,
            StorageJson = source.StorageJson,
            Document = source.Document
        };
    }

    public static AlterTaskCommand MapToAlterTaskCommand(AlterTaskRequest request, int taskId)
    {
        return new AlterTaskCommand
        {
            TaskId = taskId,
            WorkflowId = request.WorkflowId,
            Storage = request.Storage
        };
    }

    #endregion

    #region WorkflowTemplates

    public static CreateWorkflowTemplateCommand MapToCreateWorkflowTemplateCommand(WorkflowTemplateCreateRequest source)
    {
        return new CreateWorkflowTemplateCommand
        {
            TemplateName = source.TemplateName,
            WorkflowName = source.WorkflowName,
            WorkflowBaseId = source.WorkflowBaseId,
            LibraryId = source.LibraryId,
            AssociationDataJson = source.AssociationDataJson
        };
    }

    public static UpdateWorkflowTemplateCommand MapToUpdateWorkflowTemplateCommand(WorkflowTemplateUpdateRequest source, int templateId)
    {
        return new UpdateWorkflowTemplateCommand
        {
            TemplateId = templateId,
            TemplateName = source.TemplateName,
            WorkflowName = source.WorkflowName,
            WorkflowBaseId = source.WorkflowBaseId,
            AssociationDataJson = source.AssociationDataJson
        };
    }

    public static WorkflowTemplateResponse MapToWorkflowTemplateResponse(WorkflowTemplateDto dto)
    {
        return new WorkflowTemplateResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            WorkflowBaseId = dto.WorkflowBaseId,
            AssociationDataJson = dto.AssociationDataJson,
            Created = dto.Created
        };
    }

    #endregion
}