using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core.Interfaces;

namespace Uptime.Workflows.Core.Common;

public class StateTransitionQueue<TState, TTrigger>(IStateMachine<TState, TTrigger> machine, ILogger logger)
{
    private bool _isProcessingQueue;
    private readonly Queue<TTrigger> _triggerQueue = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task EnqueueTriggerAsync(TTrigger trigger, CancellationToken cancellationToken)
    {
        lock (_triggerQueue)
        {
            _triggerQueue.Enqueue(trigger);
            logger.LogInformation("Trigger {Trigger} enqueued. Queue size: {QueueSize}", trigger, _triggerQueue.Count);

            if (_isProcessingQueue)
            {
                logger.LogInformation("Trigger processing is already in progress.");
                return;
            }

            _isProcessingQueue = true;
            logger.LogInformation("Starting trigger queue processing.");
        }

        while (true)
        {
            TTrigger currentTrigger;
            lock (_triggerQueue)
            {
                if (_triggerQueue.Count == 0)
                {
                    _isProcessingQueue = false;
                    logger.LogInformation("Trigger queue is empty. Stopping processing.");
                    break;
                }

                currentTrigger = _triggerQueue.Dequeue();
                logger.LogInformation("Dequeued trigger: {Trigger}. Remaining queue size: {QueueSize}", currentTrigger, _triggerQueue.Count);
            }

            await ExecuteSynchronizedAsync(async () =>
            {
                logger.LogInformation("Processing trigger {Trigger}. Current state: {CurrentState}", currentTrigger, machine.State);
                await machine.FireAsync(currentTrigger);
                logger.LogInformation("Trigger {Trigger} fired successfully. New state: {NewState}", currentTrigger, machine.State);

            }, cancellationToken);
        }
    }

    private async Task ExecuteSynchronizedAsync(Func<Task> action, CancellationToken cancellationToken)
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