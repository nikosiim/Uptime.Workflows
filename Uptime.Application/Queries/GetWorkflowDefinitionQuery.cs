using MediatR;
using Uptime.Domain.Common;
using Uptime.Domain.Interfaces;

namespace Uptime.Application.Queries;

public record GetWorkflowDefinitionQuery : IRequest<List<WorkflowDefinition>>;

public class GetWorkflowDefinitionQueryHandler(IEnumerable<IWorkflowDefinition> workflowDefinitions)
    : IRequestHandler<GetWorkflowDefinitionQuery, List<WorkflowDefinition>>
{
    public async Task<List<WorkflowDefinition>> Handle(GetWorkflowDefinitionQuery request, CancellationToken cancellationToken)
    {
        List<WorkflowDefinition> definitions = workflowDefinitions
            .Select(wd => wd.GetDefinition())
            .ToList();

        return await Task.FromResult(definitions);
    }
}