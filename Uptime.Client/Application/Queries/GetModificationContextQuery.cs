using MediatR;
using Uptime.Client.Application.Services;
using Uptime.Shared.Common;

namespace Uptime.Client.Application.Queries;

public record GetModificationContextQuery(int WorkflowId) : IRequest<Result<string?>>;

public class GetModificationContextQueryHandler(IApiService apiService) 
    : IRequestHandler<GetModificationContextQuery, Result<string?>>
{
    public async Task<Result<string?>> Handle(GetModificationContextQuery request, CancellationToken cancellationToken)
    {
        string url = ApiRoutes.Workflows.GetModificationContext.Replace("{workflowId}", request.WorkflowId.ToString());
        Result<string?> result = await apiService.ReadAsRawStringAsync(url, cancellationToken);

        return !result.Succeeded 
            ? Result<string?>.Failure(result.Error) 
            : Result<string?>.Success(result.Value);
    }
}