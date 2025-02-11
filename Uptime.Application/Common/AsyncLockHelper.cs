namespace Uptime.Application.Common;

public class AsyncLockHelper(int initialCount = 1, int maxCount = 1)
{
    private readonly SemaphoreSlim _semaphore = new(initialCount, maxCount);

    public async Task<TResult> ExecuteSynchronizedAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await action();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ExecuteSynchronizedAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            await action();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}