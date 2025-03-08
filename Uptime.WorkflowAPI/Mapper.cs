using Uptime.Application.Commands;
using Uptime.Application.DTOs;
using Uptime.Domain.Common;
using Uptime.Shared.Models.Documents;
using Uptime.Shared.Models.Libraries;
using Uptime.Shared.Models.Tasks;
using Uptime.Shared.Models.Workflows;
using Uptime.Shared.Models.WorkflowTemplates;

namespace Uptime.WorkflowAPI;

public static class Mapper
{
    #region Enums

    public static Shared.Enums.WorkflowTaskStatus ToShared(this Domain.Enums.WorkflowTaskStatus status)
    {
        return (Shared.Enums.WorkflowTaskStatus)status;
    }

    public static Domain.Enums.WorkflowTaskStatus? ToDomain(this Shared.Enums.WorkflowTaskStatus? status)
    {
        return status != null ? (Domain.Enums.WorkflowTaskStatus)status : null;
    }

    public static Shared.Enums.WorkflowEventType ToShared(this Domain.Enums.WorkflowEventType eventType)
    {
        return (Shared.Enums.WorkflowEventType)eventType;
    }

    #endregion

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
            Outcome = dto.Outcome,
            IsActive = dto.IsActive
        }).ToList();
    }

    public static List<DocumentTasksResponse> MapToDocumentTasksResponse(List<DocumentWorkflowTaskDto> source)
    {
        return source.Select(dto => new DocumentTasksResponse
        {
            TaskId = dto.TaskId,
            WorkflowId = dto.WorkflowId,
            AssignedTo = dto.AssignedTo,
            Status = dto.InternalStatus.ToShared(),
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
    
    public static WorkflowDetailsResponse MapToWorkflowDetailsResponse(WorkflowDetailsDto source)
    {
        return new WorkflowDetailsResponse
        {
            IsActive = source.IsActive,
            Phase = source.Phase,
            Outcome = source.Outcome,
            Originator = source.Originator,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            DocumentId = source.DocumentId,
            Document = source.Document,
            WorkflowBaseId = source.WorkflowBaseId
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
            InternalStatus = dto.InternalStatus.ToShared(),
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
            DocumentId = (DocumentId)source.DocumentId,
            WorkflowTemplateId = (WorkflowTemplateId)source.WorkflowTemplateId,
            Storage = source.Storage
        };
    }

    public static List<WorkflowDefinitionResponse> MapToWorkflowDefinitionResponse(List<WorkflowDefinition>? source)
    {
        if (source is null)
            return [];

        return source.Select(wd => new WorkflowDefinitionResponse
        {
            Id = wd.Id,
            Name = wd.Name,
            DisplayName = wd.DisplayName,
            Actions = wd.Actions?.ToList(),
            Phases = wd.ReplicatorActivities?
                .Select(pa => new PhaseResponse
                {
                    PhaseId = pa.PhaseId,
                    UpdateEnabled = pa.UpdateEnabled,
                    SupportsSequential = pa.SupportsSequential,
                    SupportsParallel = pa.SupportsParallel,
                    Actions = pa.Actions.ToList()
                }).ToList()
        }).ToList();
    }
    
    public static ModificationPayload MapToModificationPayload(ModifyWorkflowRequest source)
    {
        return new ModificationPayload
        {
            Executor = source.Executor,
            ModificationContext = source.ModificationContext
        };
    }

    #endregion

    #region WorkflowTasks

    public static WorkflowTaskResponse MapToWorkflowTaskResponse(WorkflowTaskDetailsDto source)
    {
        return new WorkflowTaskResponse
        {
            Id = source.Id,
            AssignedTo = source.AssignedTo,
            AssignedBy = source.AssignedBy,
            InternalStatus = source.InternalStatus.ToShared(),
            Description = source.Description,
            DueDate = source.DueDate,
            EndDate = source.EndDate,
            StorageJson = source.StorageJson,
            Document = source.Document,
            WorkflowId = source.WorkflowId,
            PhaseId = source.PhaseId,
            WorkflowBaseId = source.WorkflowBaseId
        };
    }

    public static AlterTaskCommand MapToAlterTaskCommand(AlterTaskRequest request, int taskId)
    {
        return new AlterTaskCommand
        {
            TaskId = (TaskId)taskId,
            Payload = request.Input
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
            LibraryId = (LibraryId)source.LibraryId,
            AssociationDataJson = source.AssociationDataJson
        };
    }

    public static UpdateWorkflowTemplateCommand MapToUpdateWorkflowTemplateCommand(WorkflowTemplateUpdateRequest source, int templateId)
    {
        return new UpdateWorkflowTemplateCommand
        {
            TemplateId = (WorkflowTemplateId)templateId,
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

    public static List<WorkflowHistoryResponse> MapToWorkflowHistoryResponse(List<WorkflowHistoryDto> source)
    {
        return source.Select(dto => new WorkflowHistoryResponse
        {
            Id = dto.Id,
            WorkflowId = dto.WorkflowId,
            Occurred = dto.Occurred,
            Description = dto.Description,
            User = dto.User,
            Comment = dto.Comment,
            Event = dto.Event.ToShared()
        }).ToList();
    }
}