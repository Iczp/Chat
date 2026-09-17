using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.Ai;

public class AiRunDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IOptions<AiRunDispatcherOptions> _optionsAccessor;
    private readonly ILogger<AiRunDispatcher> _logger;
    private readonly string _workerId;
    private readonly ConcurrentDictionary<Guid, Task> _runningTasks = new();
    private SemaphoreSlim _semaphore;

    public AiRunDispatcher(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<AiRunDispatcherOptions> optionsAccessor,
        ILogger<AiRunDispatcher> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _optionsAccessor = optionsAccessor;
        _logger = logger;
        _workerId = $"{Environment.MachineName}-{Environment.ProcessId}-{Guid.NewGuid():N}";
        _semaphore = new SemaphoreSlim(Options.MaxConcurrentRuns, Options.MaxConcurrentRuns);
    }

    protected AiRunDispatcherOptions Options => _optionsAccessor.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AiRunDispatcher started with WorkerId={WorkerId}, MaxConcurrentRuns={MaxConcurrentRuns}", _workerId, Options.MaxConcurrentRuns);

        var lastReclaimTime = DateTime.MinValue;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!Options.Enabled)
                {
                    await Task.Delay(Options.PollIntervalMilliseconds, stoppingToken);
                    continue;
                }

                var now = DateTime.UtcNow;

                // 定期（每 10 秒）回收超时租约
                if ((now - lastReclaimTime).TotalSeconds >= 10)
                {
                    lastReclaimTime = now;
                    using var reclaimScope = _serviceScopeFactory.CreateScope();
                    var aiRunManager = reclaimScope.ServiceProvider.GetRequiredService<IAiRunManager>();
                    await aiRunManager.ReclaimExpiredLeasesAsync(now);
                }

                // 检查可用并发槽位
                var availableSlots = Options.MaxConcurrentRuns - _runningTasks.Count;
                if (availableSlots > 0)
                {
                    using var claimScope = _serviceScopeFactory.CreateScope();
                    var aiRunManager = claimScope.ServiceProvider.GetRequiredService<IAiRunManager>();
                    var leaseDuration = TimeSpan.FromSeconds(Math.Max(15, Options.LeaseSeconds));
                    var executionTimeout = TimeSpan.FromSeconds(Math.Max(10, Options.ExecutionTimeoutSeconds));

                    var claimedRuns = await aiRunManager.ClaimNextRunsAsync(_workerId, availableSlots, now, leaseDuration, executionTimeout);

                    foreach (var run in claimedRuns)
                    {
                        var task = ProcessRunAsync(
                            run.Id,
                            run.SourceMessageId,
                            run.SessionId,
                            run.RequesterSessionUnitId,
                            run.Provider,
                            run.AttemptCount,
                            run.DeadlineTime,
                            stoppingToken);

                        _runningTasks[run.Id] = task;
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AiRunDispatcher main loop.");
            }

            try
            {
                await Task.Delay(Options.PollIntervalMilliseconds, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("AiRunDispatcher stopping, waiting for active tasks to drain...");
        try
        {
            await Task.WhenAll(_runningTasks.Values);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while draining AiRunDispatcher tasks.");
        }
    }

    private async Task ProcessRunAsync(
        Guid runId,
        long sourceMessageId,
        Guid sessionId,
        Guid requesterSessionUnitId,
        string providerName,
        int attemptCount,
        DateTime? deadlineTime,
        CancellationToken stoppingToken)
    {
        await _semaphore.WaitAsync(stoppingToken);

        using var runCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        if (deadlineTime.HasValue)
        {
            var remaining = deadlineTime.Value - DateTime.UtcNow;
            if (remaining > TimeSpan.Zero)
            {
                runCts.CancelAfter(remaining);
            }
            else
            {
                runCts.Cancel();
            }
        }

        using var heartbeatCts = new CancellationTokenSource();
        var heartbeatTask = StartHeartbeatLoopAsync(runId, heartbeatCts.Token);

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var aiResolver = scope.ServiceProvider.GetRequiredService<IAiResolver>();
            var aiRunManager = scope.ServiceProvider.GetRequiredService<IAiRunManager>();

            var providerType = aiResolver.GetProviderOrDefault(providerName);
            if (providerType == null)
            {
                throw new InvalidOperationException($"No AI provider found registered for '{providerName}'.");
            }

            var provider = scope.ServiceProvider.GetService(providerType) as IAiProvider;
            if (provider == null)
            {
                throw new InvalidOperationException($"Could not resolve AI provider instance of type '{providerType.FullName}'.");
            }

            var runContext = new AiRunContext
            {
                RunId = runId,
                SourceMessageId = sourceMessageId,
                SessionId = sessionId,
                RequesterSessionUnitId = requesterSessionUnitId,
                Provider = providerName,
                AttemptCount = attemptCount,
                DeadlineTime = deadlineTime
            };

            var result = await provider.ExecuteAsync(runContext, runCts.Token);

            await StopHeartbeatAsync(heartbeatCts, heartbeatTask);

            if (result != null && result.Success)
            {
                await aiRunManager.CompleteAsync(runId, _workerId, result.OutputMessageId ?? 0, DateTime.UtcNow);
                _logger.LogInformation("AiRun {RunId} completed successfully. OutputMessageId={OutputMessageId}", runId, result.OutputMessageId);
            }
            else
            {
                var errorCode = result?.ErrorCode ?? "execution-failed";
                var errorMessage = result?.ErrorMessage ?? "AI execution failed without specific error message.";
                var retryDelay = CalculateRetryDelay(attemptCount);
                await aiRunManager.RetryOrFailAsync(runId, _workerId, errorCode, errorMessage, DateTime.UtcNow, retryDelay);
                _logger.LogWarning("AiRun {RunId} reported failure: {ErrorCode} - {ErrorMessage}", runId, errorCode, errorMessage);
            }
        }
        catch (OperationCanceledException ex) when (runCts.IsCancellationRequested && !stoppingToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "AiRun {RunId} timed out after deadline.", runId);
            await StopHeartbeatAsync(heartbeatCts, heartbeatTask);
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var aiRunManager = scope.ServiceProvider.GetRequiredService<IAiRunManager>();
                var retryDelay = CalculateRetryDelay(attemptCount);
                await aiRunManager.RetryOrFailAsync(runId, _workerId, "timeout", "AI execution timed out.", DateTime.UtcNow, retryDelay);
            }
            catch (Exception recordEx)
            {
                _logger.LogError(recordEx, "Failed to record timeout for AiRun {RunId}", runId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AiRun {RunId} failed with unhandled exception.", runId);
            await StopHeartbeatAsync(heartbeatCts, heartbeatTask);
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var aiRunManager = scope.ServiceProvider.GetRequiredService<IAiRunManager>();
                var retryDelay = CalculateRetryDelay(attemptCount);
                await aiRunManager.RetryOrFailAsync(runId, _workerId, ex.GetType().Name, ex.Message, DateTime.UtcNow, retryDelay);
            }
            catch (Exception recordEx)
            {
                _logger.LogError(recordEx, "Failed to record failure for AiRun {RunId}", runId);
            }
        }
        finally
        {
            await StopHeartbeatAsync(heartbeatCts, heartbeatTask);
            _runningTasks.TryRemove(runId, out _);
            _semaphore.Release();
        }
    }

    private async Task StartHeartbeatLoopAsync(Guid runId, CancellationToken cancellationToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(5, Options.HeartbeatSeconds));
        var leaseDuration = TimeSpan.FromSeconds(Math.Max(15, Options.LeaseSeconds));

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(interval, cancellationToken);
                using var scope = _serviceScopeFactory.CreateScope();
                var aiRunManager = scope.ServiceProvider.GetRequiredService<IAiRunManager>();
                await aiRunManager.HeartbeatAsync(runId, _workerId, DateTime.UtcNow, leaseDuration);
            }
        }
        catch (OperationCanceledException)
        {
            // 心跳随任务完成正常取消
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Heartbeat failed for AiRun {RunId}", runId);
        }
    }

    private static async Task StopHeartbeatAsync(CancellationTokenSource cts, Task heartbeatTask)
    {
        try
        {
            cts.Cancel();
            await heartbeatTask;
        }
        catch
        {
            // 忽略心跳任务的取消异常
        }
    }

    private TimeSpan CalculateRetryDelay(int attemptCount)
    {
        var factor = Math.Pow(Options.RetryBackoffFactor, Math.Max(0, attemptCount - 1));
        var seconds = Options.RetryFirstDelaySeconds * factor;
        return TimeSpan.FromSeconds(Math.Min(seconds, 3600));
    }
}
