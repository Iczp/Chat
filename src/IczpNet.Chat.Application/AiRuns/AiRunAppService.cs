using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Permissions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.AiRuns;

/// <inheritdoc />
public class AiRunAppService(
    IRepository<AiRun, Guid> repository,
    IAiRunManager aiRunManager) : CrudChatAppService<AiRun, AiRunDetailDto, AiRunDto, Guid, AiRunGetListInput, AiRunCreateInput, AiRunUpdateInput>(repository), IAiRunAppService
{
    protected override string GetPolicyName { get; set; } = ChatPermissions.AiRunPermissions.GetItem;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.AiRunPermissions.GetList;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.AiRunPermissions.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.AiRunPermissions.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.AiRunPermissions.Delete;
    protected virtual string RetryPolicyName { get; set; } = ChatPermissions.AiRunPermissions.Retry;
    protected virtual string CancelPolicyName { get; set; } = ChatPermissions.AiRunPermissions.Cancel;

    public IAiRunManager AiRunManager { get; } = aiRunManager;

    /// <inheritdoc />
    protected override async Task<IQueryable<AiRun>> CreateFilteredQueryAsync(AiRunGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(input.SessionId.HasValue, x => x.SessionId == input.SessionId)
            .WhereIf(input.RequesterSessionUnitId.HasValue, x => x.RequesterSessionUnitId == input.RequesterSessionUnitId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(!string.IsNullOrEmpty(input.Provider), x => x.Provider == input.Provider)
            .WhereIf(!string.IsNullOrEmpty(input.LeaseOwner), x => x.LeaseOwner == input.LeaseOwner)
            .WhereIf(input.SourceMessageId.HasValue, x => x.SourceMessageId == input.SourceMessageId)
            .WhereIf(input.OutputMessageId.HasValue, x => x.OutputMessageId == input.OutputMessageId)
            .WhereIf(input.StartCreationTime.HasValue, x => x.CreationTime >= input.StartCreationTime)
            .WhereIf(input.EndCreationTime.HasValue, x => x.CreationTime <= input.EndCreationTime);
    }

    /// <inheritdoc />
    public override async Task<AiRunDetailDto> CreateAsync(AiRunCreateInput input)
    {
        await CheckCreatePolicyAsync();

        var run = await AiRunManager.CreateAsync(
            input.SourceMessageId,
            input.SessionId,
            input.RequesterSessionUnitId,
            input.Provider,
            input.MaxAttempts ?? 3);

        return await MapToGetOutputDtoAsync(run);
    }

    /// <summary>
    /// 根据源消息 Id 获取 AI 执行记录
    /// </summary>
    public async Task<AiRunDetailDto> GetBySourceMessageIdAsync(long sourceMessageId)
    {
        await CheckGetPolicyAsync();

        var run = await (await Repository.GetQueryableAsync())
            .FirstOrDefaultAsync(x => x.SourceMessageId == sourceMessageId);

        Assert.If(run == null, $"AiRun with SourceMessageId={sourceMessageId} not found.");

        return await MapToGetOutputDtoAsync(run);
    }

    /// <summary>
    /// 获取指定会话的 AI 执行记录列表
    /// </summary>
    public async Task<PagedResultDto<AiRunDto>> GetListBySessionAsync(Guid sessionId, AiRunGetListInput input)
    {
        input.SessionId = sessionId;
        return await GetListAsync(input);
    }

    /// <summary>
    /// 人工重试执行失败或超时的任务
    /// </summary>
    public async Task<AiRunDetailDto> RetryAsync(Guid id)
    {
        await CheckPolicyAsync(RetryPolicyName);

        var run = await AiRunManager.RetryAsync(id, Clock.Now);

        return await MapToGetOutputDtoAsync(run);
    }

    /// <summary>
    /// 取消任务
    /// </summary>
    public async Task<AiRunDetailDto> CancelAsync(Guid id, string reason = null)
    {
        await CheckPolicyAsync(CancelPolicyName);

        var run = await AiRunManager.CancelAsync(id, Clock.Now, reason);

        return await MapToGetOutputDtoAsync(run);
    }
}
