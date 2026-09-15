using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionUnits;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace IczpNet.Chat.Ai;

/// <summary>Reconnect-safe IM AI runtime queries. Exposed by ABP convention as /api/chat/ai/active.</summary>
public class AiAppService(
    IAiRunStateStore aiRunStateStore,
    ISessionUnitManager sessionUnitManager) : ChatAppService, IAiAppService
{
    public async Task<AiRunStateDto> GetActiveAsync(Guid sessionUnitId)
    {
        await CheckSessionPermissionAsync(sessionUnitId);
        var state = await aiRunStateStore.GetAsync(sessionUnitId);
        return Map(state);
    }

    public async Task<List<AiRunStateDto>> GetRecentAsync(Guid sessionUnitId, int maxResultCount = 20)
    {
        await CheckSessionPermissionAsync(sessionUnitId);
        return (await aiRunStateStore.GetRecentAsync(sessionUnitId, maxResultCount)).Select(Map).ToList();
    }

    private async Task CheckSessionPermissionAsync(Guid sessionUnitId)
    {
        var unit = await sessionUnitManager.GetCacheAsync(sessionUnitId);
        await CheckPolicyForUserAsync(unit.OwnerId, () => CheckPolicyAsync(GetListPolicyName, unit.OwnerId));
    }

    private static AiRunStateDto Map(AiRunState state)
    {
        if (state == null) return null;
        return new AiRunStateDto
        {
            RunId = state.RunId, SessionId = state.SessionId, RequesterSessionUnitId = state.RequesterSessionUnitId,
            SourceMessageId = state.SourceMessageId, Status = state.Status, RequestedAt = state.RequestedAt,
            StartedAt = state.StartedAt, UpdatedAt = state.UpdatedAt, QueueMilliseconds = state.QueueMilliseconds,
            ElapsedMilliseconds = state.ElapsedMilliseconds, Sequence = state.Sequence, PreviewText = state.PreviewText,
            FinalMessageId = state.FinalMessageId, Error = state.Error,
            Timeline = state.Timeline.Select(x => new AiRunTimelineItemDto
            {
                OccurredAt = x.OccurredAt, EventType = x.EventType, Status = x.Status,
                Sequence = x.Sequence, ElapsedMilliseconds = x.ElapsedMilliseconds, Detail = x.Detail
            }).ToList()
        };
    }
}
