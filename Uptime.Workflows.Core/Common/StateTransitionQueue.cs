using Microsoft.Extensions.Logging;
using Stateless;

namespace Uptime.Workflows.Core.Common;

public class StateTransitionQueue<TState, TTrigger>(StateMachine<TState, TTrigger> machine, ILogger logger)
{
    private bool _isProcessingQueue;
    private readonly Queue<TTrigger> _triggerQueue = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task EnqueueTriggerAsync(TTrigger trigger, CancellationToken ct)
    {
        lock (_triggerQueue)
        {
            _triggerQueue.Enqueue(trigger);
            logger.LogTriggerEnqueued(trigger, _triggerQueue.Count);

            if (_isProcessingQueue)
            {
                logger.LogTriggerProcessingAlreadyInProgress();
                return;
            }

            _isProcessingQueue = true;
            logger.LogTriggerProcessingStarted();
        }

        while (true)
        {
            TTrigger currentTrigger;
            lock (_triggerQueue)
            {
                if (_triggerQueue.Count == 0)
                {
                    _isProcessingQueue = false;
                    logger.LogTriggerQueueEmpty();
                    break;
                }

                currentTrigger = _triggerQueue.Dequeue();
                logger.LogTriggerDequeued(currentTrigger, _triggerQueue.Count);
            }

            await ExecuteSynchronizedAsync(async () =>
            {
                logger.LogTriggerProcessing(currentTrigger, machine.State);
                await machine.FireAsync(currentTrigger);
                logger.LogTriggerFired(currentTrigger, machine.State);

            }, ct);
        }
    }

    private async Task ExecuteSynchronizedAsync(Func<Task> action, CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);

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