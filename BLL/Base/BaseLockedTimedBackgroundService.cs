using Microsoft.Extensions.Logging;

namespace BLL.Base;

public abstract class BaseLockedTimedBackgroundService : BaseTimedBackgroundService
{
    private readonly object _lock = new();
    private bool _isRunning;

    protected BaseLockedTimedBackgroundService(ILogger logger, IServiceProvider services, TimeSpan period) :
        base(logger, services, period)
    {
    }

    protected abstract Task DoLockedWork(object? state);
    protected virtual Task AfterLockedWork() => Task.CompletedTask;

    protected override async void DoWork(object? state)
    {
        lock (_lock)
        {
            if (_isRunning)
            {
                Logger.LogInformation($"Previous execution of background worker still running. Skipping execution.");
                return;
            }

            _isRunning = true;
        }

        try
        {
            await DoLockedWork(state);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Exception occurred when executing background service {ServiceType}.", GetType());
        }
        finally
        {
            lock (_lock)
            {
                _isRunning = false;
            }
        }

        await AfterLockedWork();
    }
}