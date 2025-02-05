using MediatR;
using System.Text.Json;
using Uptime.Application.Commands;
using Uptime.Application.DTOs;
using Uptime.Application.Enums;
using Uptime.Application.Interfaces;
using Uptime.Application.Models.Approval;
using Uptime.Application.Models.Common;
using Uptime.Application.Queries;
using Uptime.Application.Workflows;
using Uptime.Shared.Enums;

namespace Uptime.Application.Services;

public class WorkflowService(ITaskService taskService, IMediator mediator) : IWorkflowService
{
    public async Task<bool> StartWorkflowAsync<TPayload>(TPayload inputData) where TPayload : class
    {
        if (inputData is ApprovalWorkflowPayload approvalPayload)
        {
            return await HandleApprovalWorkflowAsync(approvalPayload);
        }

        throw new InvalidOperationException("Unknown workflow type");
    }

    public async Task<bool> UpdateWorkflowTaskAsync(AlterTaskPayload payload)
    {
        // Retrieve workflow instance
        WorkflowDto? workflowInstance = await mediator.Send(new GetWorkflowQuery(payload.WorkflowId));
        if (workflowInstance == null)
            return false;

        // Deserialize the workflow context
        var workflowContext = JsonSerializer.Deserialize<ApprovalWorkflowContext>(workflowInstance.InstanceDataJson ?? string.Empty);
        if (workflowContext == null)
            return false;

        // Instantiate the workflow
        var workflow = new ApprovalWorkflow(taskService, workflowInstance.Status, workflowContext);

        // Let workflow handle task update
        bool taskHandled = await workflow.CompleteTaskAsync(payload);
        if (!taskHandled)
            return false;

        // Persist updated workflow state
        workflowInstance = workflowInstance with
        {
            InstanceDataJson = JsonSerializer.Serialize(workflow.WorkflowContext)
        };

        await mediator.Send(new UpdateWorkflowStateCommand
        {
            WorkflowId = payload.WorkflowId,
            Status = workflow.CurrentState,
            InstanceDataJson = workflowInstance.InstanceDataJson
        });

        return true;
    }

    #region Implementations
    
    private async Task<bool> HandleApprovalWorkflowAsync(ApprovalWorkflowPayload payload)
    {
        var createCmd = new CreateWorkflowInstanceCommand
        {
            Originator = payload.Originator,
            DocumentId = payload.DocumentId,
            WorkflowTemplateId = payload.WorkflowTemplateId
        };
        
        // Store the workflow instance before starting it
        int workflowId = await mediator.Send(createCmd);
        
        var workflowContext = new ApprovalWorkflowContext
        {
            ReplicatorState = new ReplicatorState<ApprovalTaskContext>
            {
                Type = ReplicatorType.Sequential,
                Items = GetApprovalTasks(workflowId, payload)
            }
        };
        
        var workflow = new ApprovalWorkflow(taskService, WorkflowStatus.NotStarted, workflowContext);

        await workflow.FireAsync(WorkflowTrigger.Start);

        await UpdateWorkflowInstanceAsync(workflowId, workflow.CurrentState, workflow.Context);

        return true;
    }
    
    private static List<ApprovalTaskContext> GetApprovalTasks(int workflowId, ApprovalWorkflowPayload payload)
    {
        List<ApprovalTaskContext> approvalTasks = [];

        foreach (string executor in payload.Executors)
        {
            approvalTasks.Add(new ApprovalTaskContext
            {
                WorkflowId = workflowId,
                AssignedBy = payload.Originator,
                AssignedTo = executor,
                TaskDescription = payload.Description,
                DueDate = payload.DueDate
            });
        }     

        return approvalTasks;
    }

    private async Task UpdateWorkflowInstanceAsync<T>(int workflowId, WorkflowStatus status, T workflowContext)
    {
        var cmd = new UpdateWorkflowStateCommand
        {
            WorkflowId = workflowId,
            Status = status,
            InstanceDataJson = JsonSerializer.Serialize(workflowContext)
        };
      
        await mediator.Send(cmd);
    }

    #endregion
}