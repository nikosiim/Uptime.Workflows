﻿namespace Uptime.Workflows.Application.Messaging
{
    public interface ISender
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
    }
}