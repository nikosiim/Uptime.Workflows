using MediatR;
using Uptime.Client.Application.Common;
using Uptime.Client.Application.Services;
using Uptime.Client.Contracts;

namespace Uptime.Client.Application.Commands;

public record CreateWorkflowTemplateCommand : IRequest<Result<int>>
{
    public required string TemplateName { get; init; }
    public required string WorkflowName { get; init; }
    public required string WorkflowBaseId { get; init; }
    public required int LibraryId { get; init; }
    public string? AssociationDataJson { get; init; }
}

public class CreateWorkflowTemplateCommandHandler(IApiService apiService)
    : IRequestHandler<CreateWorkflowTemplateCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateWorkflowTemplateCommand request, CancellationToken cancellationToken)
    {
        var createRequest = new WorkflowTemplateCreateRequest {
            SourceSiteUrl = "https://uptimesharepoint",
            TemplateName = request.TemplateName,
            WorkflowName = request.WorkflowName,
            WorkflowBaseId = request.WorkflowBaseId,
            LibraryId = request.LibraryId,
            AssociationDataJson = request.AssociationDataJson ?? string.Empty
        };

        Result<CreateWorkflowTemplateResponse> result = await apiService.CreateAsync<object, CreateWorkflowTemplateResponse>(
            ApiRoutes.WorkflowTemplates.CreateTemplate, createRequest, cancellationToken);
      
        return result.Succeeded
            ? Result<int>.Success(result.Value?.Id ?? 0)
            : Result<int>.Failure(result.Error);
    }
}