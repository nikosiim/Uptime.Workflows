﻿namespace Uptime.Workflows.Application.Messaging
{
    public interface IRequest<TResponse>;

    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken ct);
    }
}