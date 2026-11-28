using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.AiRuns.Dtos;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.AiRuns;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.AiRuns;

/// <summary>
/// AI 任务执行记录管理服务
/// </summary>
[Authorize(ChatAdminPermissions.AiRuns.Default)]
[ExposeServices(typeof(AiRunAppService), typeof(IAiRunAdminAppService))]
public class AiRunAppService(IAiRunManager aiRunManager)
    : ApplicationService, IAiRunAdminAppService
{
    protected IAiRunManager AiRunManager { get; } = aiRunManager;

    [Authorize(ChatAdminPermissions.AiRuns.GetList)]
    public virtual async Task<PagedResultDto<AiRunAdminDto>> GetListAsync(
        AiRunAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new AiRunAdminGetListInput();

        var result = await AiRunManager.GetAdminListAsync(
            input.Keyword,
            input.Status,
            input.Provider,
            input.SessionId,
            input.RequesterSessionUnitId,
            input.SourceMessageId,
            input.OutputMessageId,
            input.LeaseOwner,
            input.StartCreationTime,
            input.EndCreationTime,
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            cancellationToken);

        return new PagedResultDto<AiRunAdminDto>(
            result.TotalCount,
            result.Items.Select(MapToDto).ToList());
    }

    [Authorize(ChatAdminPermissions.AiRuns.GetDetail)]
    public virtual async Task<AiRunAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await AiRunManager.GetAdminDetailAsync(id, cancellationToken);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.AiRuns.GetDetail)]
    public virtual async Task<AiRunAdminDto> GetBySourceMessageIdAsync(long sourceMessageId)
    {
        var entity = await AiRunManager.FindBySourceMessageIdAsync(sourceMessageId);
        return entity == null ? null : MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.AiRuns.GetList)]
    public virtual async Task<PagedResultDto<AiRunAdminDto>> GetBySessionAsync(
        Guid sessionId,
        AiRunAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new AiRunAdminGetListInput();
        input.SessionId = sessionId;
        return await GetListAsync(input, cancellationToken);
    }

    [Authorize(ChatAdminPermissions.AiRuns.Retry)]
    public virtual async Task<AiRunAdminDto> CreateAsync(AiRunAdminCreateInput input)
    {
        var entity = await AiRunManager.CreateAsync(
            input.SourceMessageId,
            input.SessionId,
            input.RequesterSessionUnitId,
            input.Provider,
            input.MaxAttempts);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.AiRuns.Retry)]
    public virtual async Task<AiRunAdminDto> RetryAsync(Guid id)
    {
        var entity = await AiRunManager.RetryAsync(id, Clock.Now);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.AiRuns.Cancel)]
    public virtual async Task<AiRunAdminDto> CancelAsync(Guid id, AiRunAdminCancelInput input = null)
    {
        var entity = await AiRunManager.CancelAsync(id, Clock.Now, input?.Reason);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.AiRuns.Delete)]
    public virtual Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return AiRunManager.DeleteAdminAsync(id, cancellationToken);
    }

    [Authorize(ChatAdminPermissions.AiRuns.Delete)]
    public virtual Task DeleteManyAsync([FromQuery] List<Guid> idList, CancellationToken cancellationToken = default)
    {
        return AiRunManager.DeleteManyAdminAsync(idList, cancellationToken);
    }

    private static AiRunAdminDto MapToDto(AiRun entity)
    {
        return new AiRunAdminDto
        {
            Id = entity.Id,
            SourceMessageId = entity.SourceMessageId,
            SessionId = entity.SessionId,
            RequesterSessionUnitId = entity.RequesterSessionUnitId,
            Provider = entity.Provider,
            Status = entity.Status,
            AttemptCount = entity.AttemptCount,
            MaxAttempts = entity.MaxAttempts,
            NextAttemptAt = entity.NextAttemptAt,
            LeaseOwner = entity.LeaseOwner,
            LeaseUntilTime = entity.LeaseUntilTime,
            LastHeartbeatTime = entity.LastHeartbeatTime,
            StartedTime = entity.StartedTime,
            CompletedTime = entity.CompletedTime,
            DeadlineTime = entity.DeadlineTime,
            OutputMessageId = entity.OutputMessageId,
            LastErrorCode = entity.LastErrorCode,
            LastErrorMessage = entity.LastErrorMessage,
            CreationTime = entity.CreationTime,
            LastModificationTime = entity.LastModificationTime
        };
    }
}
