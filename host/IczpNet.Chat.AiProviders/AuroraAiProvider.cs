using Aurora.AiConnector;
using IczpNet.Ai;
using IczpNet.Chat.Ai;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;
using IczpNet.Chat.AiRuns;

namespace IczpNet.Chat.AiProviders;

/// <summary>
/// Bridges Rctea IM messages to Aurora's server-to-server Connector API.
/// The response is consumed as SSE and emitted in readable message batches.
/// </summary>
[Ai("Aurora")]
public class AuroraAiProvider(
    IAuroraAiConnectorClient aurora,
    IConfiguration configuration,
    IDistributedEventBus distributedEventBus,
    IAiRunStateStore aiRunStateStore) : AiProvider
{
    private readonly IAuroraAiConnectorClient _aurora = aurora;
    private readonly IConfiguration _configuration = configuration;
    private readonly IDistributedEventBus _distributedEventBus = distributedEventBus;
    private readonly IAiRunStateStore _aiRunStateStore = aiRunStateStore;

    public override string GetModel() => "Aurora AI Platform";

    [UnitOfWork]
    public override async Task<AiRunExecutionResult> ExecuteAsync(AiRunContext context, CancellationToken cancellationToken = default)
    {
        return await ExecuteInternalAsync(context.SourceMessageId, context.RunId.ToString("N"), cancellationToken);
    }

    [UnitOfWork]
    public override async Task HandleAsync(long messageId)
    {
        await ExecuteInternalAsync(messageId, Guid.NewGuid().ToString("N"), CancellationToken.None);
    }

    private async Task<AiRunExecutionResult> ExecuteInternalAsync(long messageId, string runId, CancellationToken cancellationToken)
    {
        var message = await MessageRepository.GetAsync(messageId);
        if (message.IsRollbackMessage())
        {
            Logger.LogInformation("Aurora ignored rollback message {MessageId}", messageId);
            return AiRunExecutionResult.Ok();
        }

        var replySessionUnit = await SessionUnitManager.FindCacheAsync(message.ReceiverId.Value, message.SenderId.Value);
        if (replySessionUnit == null)
        {
            Logger.LogWarning("Aurora reply session is unavailable for message {MessageId}", messageId);
            return AiRunExecutionResult.Fail("session-unavailable", "Aurora reply session is unavailable.");
        }

        var text = message.GetTypedContentEntity()?.GetBody()?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            await SendTextAsync(replySessionUnit, messageId, new TextContentInfo { Text = "暂时只支持文本消息，请发送文字内容。" });
            return AiRunExecutionResult.Ok();
        }

        var quoteText = message.QuoteMessage?.GetTypedContentEntity()?.GetBody()?.Trim();
        if (!string.IsNullOrEmpty(quoteText)) text += "\n\n[引用消息]\n" + quoteText;

        var chatSessionId = message.SessionId ?? throw new InvalidOperationException($"Aurora message {messageId} has no SessionId.");
        var requesterSessionUnitId = message.SenderSessionUnitId ?? throw new InvalidOperationException($"Aurora message {messageId} has no sender SessionUnitId.");
        var streamTimeoutSeconds = Math.Max(10, _configuration.GetValue("Aurora:StreamTimeoutSeconds", 60));
        using var streamTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(streamTimeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, streamTimeout.Token);
        var timing = new AiRunTiming(message.CreationTime);
        var sequence = 0;

        try
        {
            // Do not create a permanent "thinking" message or partial reply.
            // Flutter renders this distributed ChatHub event in memory; only
            // the completed answer goes through the normal persisted message path.
            await PublishAiEventAsync(new AiStreamStartedCommandPayload
            {
                RunId = runId,
                SessionId = chatSessionId,
                RequesterSessionUnitId = requesterSessionUnitId,
                SourceMessageId = messageId,
                Sequence = 0
            }, timing);
            var externalSessionId = message.SessionId.ToString();
            Logger.LogInformation("Aurora AI registering session for IM message {MessageId}; baseUrl={BaseUrl}; runId={RunId}", messageId, _configuration["Aurora:BaseUrl"], runId);
            var session = await _aurora.RegisterSessionAsync(new AuroraSessionRequest
            {
                ExternalSessionId = externalSessionId,
                ExternalSubject = message.Sender.Id.ToString(),
                DisplayName = message.Sender.Name ?? string.Empty,
                Channel = "rctea-im",
                ConversationType = "direct",
                Profile = new AuroraExternalProfile
                {
                    Attributes = new Dictionary<string, object>
                    {
                        ["name"] = message.Sender.Name ?? string.Empty,
                        ["gender"] = message.Sender.Gender.ToString(),
                        ["rctea_user_id"] = message.Sender.Id.ToString()
                    },
                    ContextKeys = new List<string> { "name", "gender" },
                    SensitiveKeys = new List<string> { "rctea_user_id" }
                }
            }, linkedCts.Token);

            Logger.LogInformation("Aurora AI session registered for IM message {MessageId}; runId={RunId}; starting SSE stream", messageId, runId);

            var flushCharacters = Math.Max(20, _configuration.GetValue("Aurora:StreamFlushCharacters", 40));
            var pending = new StringBuilder();
            var answer = new StringBuilder();
            var receivedAnyOutput = false;
            var flushWatch = Stopwatch.StartNew();

            async Task FlushTransientAsync(bool force = false)
            {
                if (pending.Length == 0) return;
                // Limit Pusher traffic while preserving a natural streaming UI.
                if (!force && pending.Length < flushCharacters && flushWatch.ElapsedMilliseconds < 120) return;
                var chunk = pending.ToString();
                pending.Clear();
                flushWatch.Restart();
                await PublishAiEventAsync(new AiStreamDeltaCommandPayload
                {
                    RunId = runId,
                    SessionId = chatSessionId,
                    RequesterSessionUnitId = requesterSessionUnitId,
                    SourceMessageId = messageId,
                    Sequence = ++sequence,
                    Delta = chunk
                }, timing);
            }

            Logger.LogInformation("Aurora AI opening SSE stream for IM message {MessageId}; baseUrl={BaseUrl}; runId={RunId}", messageId, _configuration["Aurora:BaseUrl"], runId);
            await _aurora.SendMessageAsync(new AuroraMessageRequest
            {
                SessionId = session.Id,
                SourceMessageId = messageId.ToString(),
                ConversationId = externalSessionId,
                ConversationType = "direct",
                SenderId = message.Sender.Id.ToString(),
                Content = text
            }, async streamEvent =>
            {
                if (streamEvent.IsError)
                {
                    var trace = string.IsNullOrWhiteSpace(streamEvent.TraceId) ? string.Empty : $"；TraceId={streamEvent.TraceId}";
                    var execution = string.IsNullOrWhiteSpace(streamEvent.ExecutionId) ? string.Empty : $"；ExecutionId={streamEvent.ExecutionId}";
                    throw new InvalidOperationException($"Aurora AI 错误事件：{streamEvent.ErrorMessage ?? "未提供错误描述"}{trace}{execution}");
                }
                var delta = streamEvent.TextDelta;
                if (string.IsNullOrEmpty(delta)) return;
                receivedAnyOutput = true;
                pending.Append(delta);
                answer.Append(delta);
                await FlushTransientAsync();
            }, linkedCts.Token);

            await FlushTransientAsync(force: true);
            if (!receivedAnyOutput)
            {
                await SendReplyAndLogAsync(replySessionUnit, messageId, "AI 未返回可显示的内容，请稍后重试。", "empty");
                await PublishAiEventAsync(new AiStreamFailedCommandPayload
                {
                    RunId = runId,
                    SessionId = chatSessionId,
                    RequesterSessionUnitId = requesterSessionUnitId,
                    SourceMessageId = messageId,
                    Sequence = ++sequence,
                    Error = "AI 未返回可显示的内容。"
                }, timing);
                return AiRunExecutionResult.Fail("no-output", "AI 未返回可显示的内容。");
            }

            var finalReply = await SendReplyAndLogAsync(replySessionUnit, messageId, answer.ToString(), "completed");
            await PublishAiEventAsync(new AiStreamCompletedCommandPayload
            {
                RunId = runId,
                SessionId = chatSessionId,
                RequesterSessionUnitId = requesterSessionUnitId,
                SourceMessageId = messageId,
                Sequence = ++sequence,
                FinalMessageId = finalReply.Id
            }, timing);
            Logger.LogInformation("Aurora AI run completed for IM message {MessageId}; runId={RunId}; queue={QueueMilliseconds}ms; elapsed={ElapsedMilliseconds}ms", messageId, runId, timing.QueueMilliseconds, timing.ElapsedMilliseconds);
            return AiRunExecutionResult.Ok(finalReply.Id);
        }
        catch (OperationCanceledException exception) when (linkedCts.IsCancellationRequested)
        {
            Logger.LogError(exception, "Aurora Connector stream timed out for IM message {MessageId} after {TimeoutSeconds}s; runId={RunId}; queue={QueueMilliseconds}ms; elapsed={ElapsedMilliseconds}ms", messageId, streamTimeoutSeconds, runId, timing.QueueMilliseconds, timing.ElapsedMilliseconds);
            await PublishAiEventAsync(new AiStreamFailedCommandPayload { RunId = runId, SessionId = chatSessionId, RequesterSessionUnitId = requesterSessionUnitId, SourceMessageId = messageId, Sequence = ++sequence, Error = $"AI 响应超时（等待超过 {streamTimeoutSeconds} 秒）。" }, timing);
            await SendFailureReplyAsync(replySessionUnit, messageId, $"AI 响应超时（等待超过 {streamTimeoutSeconds} 秒）。");
            return AiRunExecutionResult.Fail("timeout", $"AI 响应超时（等待超过 {streamTimeoutSeconds} 秒）。");
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Aurora Connector failed for IM message {MessageId}; runId={RunId}; queue={QueueMilliseconds}ms; elapsed={ElapsedMilliseconds}ms", messageId, runId, timing.QueueMilliseconds, timing.ElapsedMilliseconds);
            var detail = GetSafeErrorDetail(exception);
            await PublishAiEventAsync(new AiStreamFailedCommandPayload { RunId = runId, SessionId = chatSessionId, RequesterSessionUnitId = requesterSessionUnitId, SourceMessageId = messageId, Sequence = ++sequence, Error = detail }, timing);
            await SendFailureReplyAsync(replySessionUnit, messageId, $"{detail}\n请根据该错误信息重试或联系管理员查看服务日志。");
            return AiRunExecutionResult.Fail(exception.GetType().Name, detail);
        }
    }

    private async Task SendFailureReplyAsync(SessionUnitCacheItem replySessionUnit, long messageId, string detail)
    {
        try
        {
            await SendReplyAndLogAsync(replySessionUnit, messageId, $"AI 调用失败（消息 {messageId}）：{detail}", "failure");
        }
        catch (Exception replyException)
        {
            Logger.LogError(replyException, "Aurora failure reply could not be sent for IM message {MessageId}", messageId);
        }
    }

    private async Task<MessageInfo<TextContentInfo>> SendReplyAndLogAsync(SessionUnitCacheItem replySessionUnit, long messageId, string text, string stage)
    {
        var reply = await SendTextAsync(replySessionUnit, messageId, new TextContentInfo { Text = text });
        Logger.LogInformation("Aurora IM reply sent for message {MessageId}; stage={Stage}; reply={Reply}", messageId, stage, reply);
        return reply;
    }

    private async Task PublishAiEventAsync(AiStreamCommandPayload payload, AiRunTiming timing)
    {
        try
        {
            var eventData = new AiStreamToClientDistributedEto
            {
                Command = payload switch
                {
                    AiStreamStartedCommandPayload => IczpNet.Chat.Commands.CommandConsts.AiStreamStarted,
                    AiStreamDeltaCommandPayload => IczpNet.Chat.Commands.CommandConsts.AiStreamDelta,
                    AiStreamCompletedCommandPayload => IczpNet.Chat.Commands.CommandConsts.AiStreamCompleted,
                    AiStreamFailedCommandPayload => IczpNet.Chat.Commands.CommandConsts.AiStreamFailed,
                    _ => throw new InvalidOperationException($"Unsupported Aurora AI stream event: {payload.GetType().Name}")
                },
                RunId = payload.RunId,
                SessionId = payload.SessionId,
                RequesterSessionUnitId = payload.RequesterSessionUnitId,
                SourceMessageId = payload.SourceMessageId,
                RequestedAt = timing.RequestedAt,
                StartedAt = timing.StartedAt,
                OccurredAt = DateTime.UtcNow,
                QueueMilliseconds = timing.QueueMilliseconds,
                ElapsedMilliseconds = timing.ElapsedMilliseconds,
                Sequence = payload.Sequence,
                Status = (payload as AiStreamStartedCommandPayload)?.Status,
                Delta = (payload as AiStreamDeltaCommandPayload)?.Delta,
                FinalMessageId = (payload as AiStreamCompletedCommandPayload)?.FinalMessageId,
                Error = (payload as AiStreamFailedCommandPayload)?.Error
            };
            await PersistRunStateAsync(eventData);
            await _distributedEventBus.PublishAsync(eventData, onUnitOfWorkComplete: false);
        }
        catch (Exception exception)
        {
            // A transient UI event must never discard an AI answer or fail its job.
            Logger.LogWarning(exception, "Aurora ChatHub stream event {EventType} could not be published for source message {MessageId}", payload.GetType().Name, payload.SourceMessageId);
        }
    }

    private async Task PersistRunStateAsync(AiStreamToClientDistributedEto eventData)
    {
        // The store folds this event in Redis Lua. Do not read and mutate a
        // snapshot here: parallel SSE deltas may otherwise overwrite each
        // other's preview text or timeline entries.
        await _aiRunStateStore.ApplyEventAsync(new AiRunStateEvent
        {
            RunId = eventData.RunId,
            SessionId = eventData.SessionId,
            RequesterSessionUnitId = eventData.RequesterSessionUnitId,
            SourceMessageId = eventData.SourceMessageId,
            RequestedAt = eventData.RequestedAt,
            StartedAt = eventData.StartedAt,
            OccurredAt = eventData.OccurredAt,
            QueueMilliseconds = eventData.QueueMilliseconds,
            ElapsedMilliseconds = eventData.ElapsedMilliseconds,
            Sequence = eventData.Sequence,
            Status = eventData.Command switch
            {
                IczpNet.Chat.Commands.CommandConsts.AiStreamStarted => "running",
                IczpNet.Chat.Commands.CommandConsts.AiStreamDelta => "streaming",
                IczpNet.Chat.Commands.CommandConsts.AiStreamCompleted => "completed",
                IczpNet.Chat.Commands.CommandConsts.AiStreamFailed => "failed",
                _ => "running"
            },
            EventType = eventData.Command,
            Delta = eventData.Delta,
            FinalMessageId = eventData.FinalMessageId,
            Error = eventData.Error
        });
    }

    private static string GetSafeErrorDetail(Exception exception)
    {
        var detail = exception.GetBaseException().Message?.Trim();
        if (string.IsNullOrWhiteSpace(detail)) detail = exception.GetType().Name;
        // OAuth credentials are never expected in an exception, but prevent an
        // accidental configuration/HTTP dump from being sent into an IM room.
        detail = Regex.Replace(detail, "(?i)(client_secret|access_token)\\s*[=:]\\s*[^,\\s\\\"}]+", "$1=[REDACTED]");
        return detail.Length > 800 ? detail.Substring(0, 800) + "…" : detail;
    }

    private sealed class AiRunTiming
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public AiRunTiming(DateTime requestedAt)
        {
            RequestedAt = requestedAt;
            StartedAt = DateTime.UtcNow;
            var requestedUtc = requestedAt.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(requestedAt, DateTimeKind.Utc)
                : requestedAt.ToUniversalTime();
            QueueMilliseconds = Math.Max(0, (long)(StartedAt - requestedUtc).TotalMilliseconds);
        }

        public DateTime RequestedAt { get; }
        public DateTime StartedAt { get; }
        public long QueueMilliseconds { get; }
        public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;
    }
}
