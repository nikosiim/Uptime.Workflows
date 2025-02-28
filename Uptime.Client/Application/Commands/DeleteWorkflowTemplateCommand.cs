using MediatR;
using Uptime.Client.Application.Services;
using Uptime.Shared.Common;

namespace Uptime.Client.Application.Commands;

public record DeleteWorkflowTemplateCommand(int TemplateId) : IRequest<Result<bool>>;

public class DeleteWorkflowTemplateCommandHandler(IApiService apiService)
    : IRequestHandler<DeleteWorkflowTemplateCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteWorkflowTemplateCommand request, CancellationToken cancellationToken)
    {
        string url = ApiRoutes.WorkflowTemplates.DeleteTemplate.Replace("{templateId}", request.TemplateId.ToString());
        return await apiService.DeleteAsync(url, cancellationToken);
    }
}