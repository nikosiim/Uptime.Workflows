using Uptime.Workflows.Api.Contracts;
using Uptime.Workflows.Application.DTOs;
using DomainWorkflowDefinition = Uptime.Workflows.Core.WorkflowDefinition;
using DomainWorkflowEventType = Uptime.Workflows.Core.Enums.WorkflowEventType;
using DomainWorkflowTaskStatus = Uptime.Workflows.Core.Enums.WorkflowTaskStatus;

namespace Uptime.Workflows.Api;

public static class EnumMapper
{
    public static DomainWorkflowTaskStatus? MapToDomain(WorkflowTaskStatus? status)
    {
        return status.HasValue ? (DomainWorkflowTaskStatus)status.Value : null;
    }

    public static DomainWorkflowEventType MapToDomain(WorkflowEventType action)
    {
        return (DomainWorkflowEventType)action;
    }
}

public static class Mapper
{
    public static List<DocumentTasksResponse> MapToDocumentTasksResponse(List<DocumentWorkflowTaskDto> source)
    {
        return source.Select(dto => new DocumentTasksResponse
        {
            TaskId = dto.TaskId,
            WorkflowId = dto.WorkflowId,
            AssignedTo = dto.AssignedTo,
            WorkflowTaskStatus = dto.WorkflowTaskStatus.ToString(),
            TaskDescription = dto.TaskDescription,
            DueDate = dto.DueDate,
            EndDate = dto.EndDate
        }).ToList();
    }
    
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
            TaskGuid = dto.TaskGuid,
            AssignedTo = dto.AssignedTo,
            AssignedBy = dto.AssignedBy,
            DisplayStatus = dto.DisplayStatus,
            InternalStatus = (int)dto.InternalStatus,
            Description = dto.Description,
            DueDate = dto.DueDate ?? DateTime.UtcNow, // Default to current UTC time if null
            EndDate = dto.EndDate,
            StorageJson = dto.StorageJson
        }).ToList();
    }
    
    public static List<WorkflowDefinitionResponse> MapToWorkflowDefinitionResponse(List<DomainWorkflowDefinition>? source)
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
    
    public static WorkflowTaskResponse MapToWorkflowTaskResponse(WorkflowTaskDetailsDto source)
    {
        return new WorkflowTaskResponse
        {
            TaskGuid = source.TaskGuid,
            AssignedTo = source.AssignedTo,
            AssignedBy = source.AssignedBy,
            InternalStatus = (int)source.InternalStatus,
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
    
    public static WorkflowTemplateResponse MapToWorkflowTemplateResponse(WorkflowTemplateDto dto)
    {
        return new WorkflowTemplateResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            WorkflowBaseId = dto.WorkflowBaseId,
            AssociationDataJson = dto.AssociationDataJson,
            Created = dto.CreatedAtUtc
        };
    }
}