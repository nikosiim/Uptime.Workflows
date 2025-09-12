using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core;

public static class WorkflowPayloadExtensions
{
    public static void CopyToContext(this IWorkflowPayload payload, IWorkflowContext context)
    {
        context.Storage.MergeWith(payload.Storage);

        context.Storage[WorkflowStorageKeys.DocumentId] = payload.DocumentId.Value.ToString();
        context.Storage[WorkflowStorageKeys.WorkflowTemplateId] = payload.WorkflowTemplateId.Value.ToString();  

        if (!string.IsNullOrWhiteSpace(payload.PrincipalSid))
            context.Storage[WorkflowStorageKeys.Initiator.Sid] = payload.PrincipalSid;
    }
}