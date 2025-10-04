namespace Workflows.Application.Messaging
{
    public interface IPipelineBehavior<in TRequest, TResponse> 
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken ct);
    }
}