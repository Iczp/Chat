using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.QrCodes.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace IczpNet.Chat.QrCodes;

/// <summary>
/// 通用二维码（个人/客服名片、群二维码、应用分享等）
/// </summary>
public class QrCodeAppService(
    IRepository<QrCode, Guid> repository,
    IQrCodeManager qrCodeManager,
    IOptions<QrCodeOptions> options) : CrudChatAppService<
        QrCode,
        QrCodeDetailDto,
        QrCodeDto,
        Guid,
        QrCodeGetListInput>(repository), IQrCodeAppService
{
    protected override string GetPolicyName { get; set; } = ChatPermissions.QrCodePermissions.GetItem;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.QrCodePermissions.GetList;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.QrCodePermissions.Delete;

    protected IQrCodeManager QrCodeManager { get; } = qrCodeManager;
    protected QrCodeOptions Options { get; } = options.Value;

    protected override async Task<IQueryable<QrCode>> CreateFilteredQueryAsync(QrCodeGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
            .WhereIf(input.InviterId.HasValue, x => x.InviterId == input.InviterId)
            .WhereIf(input.Scene.HasValue, x => x.Scene == input.Scene)
            .WhereIf(input.IsPermanent.HasValue, x => x.IsPermanent == input.IsPermanent)
            .WhereIf(input.IsEnabled.HasValue, x => x.IsEnabled == input.IsEnabled)
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Title.Contains(input.Keyword) || x.Code.Contains(input.Keyword));
    }

    protected override async Task CheckGetListPolicyAsync(QrCodeGetListInput input)
    {
        if (await IsOwnedAnyAsync(input.OwnerId, input.InviterId))
        {
            return;
        }

        await base.CheckGetListPolicyAsync(input);
    }

    [HttpGet]
    public override async Task<QrCodeDetailDto> GetAsync(Guid id)
    {
        var entity = await GetEntityByIdAsync(id);
        if (!await IsOwnedAnyAsync(entity.OwnerId, entity.InviterId))
        {
            await CheckGetPolicyAsync();
        }

        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<QrCodeDetailDto> MapToGetOutputDtoAsync(QrCode entity)
    {
        var dto = ObjectMapper.Map<QrCode, QrCodeDetailDto>(entity);
        dto.IsExpired = entity.IsExpired(Clock.Now);

        var scanInfo = await QrCodeManager.ResolveByCodeAsync(entity.Code);
        if (scanInfo != null)
        {
            dto.OwnerName = scanInfo.OwnerName;
            dto.OwnerCode = scanInfo.OwnerCode;
            dto.OwnerPortrait = scanInfo.OwnerPortrait;
            dto.OwnerThumbnail = scanInfo.OwnerThumbnail;
            dto.InviterName = scanInfo.InviterName;
        }

        return dto;
    }

    /// <summary>
    /// 获取或生成名片/群聊/自定义二维码
    /// </summary>
    [HttpPost]
    public virtual async Task<QrCodeDetailDto> GenerateAsync(QrCodeGenerateInput input)
    {
        QrCode entity;
        if (input.Scene.HasValue && input.Scene.Value is QrCodeScenes.WorkbenchApp or QrCodeScenes.CustomPayload)
        {
            if (!input.OwnerId.HasValue)
            {
                await CheckPolicyAsync(ChatPermissions.QrCodePermissions.Generate);
            }
            else if (!await IsOwnedAnyAsync(input.OwnerId))
            {
                await CheckPolicyAsync(ChatPermissions.QrCodePermissions.Generate);
            }

            entity = await QrCodeManager.GenerateCustomAsync(
                input.Scene.Value,
                input.OwnerId,
                input.InviterId,
                input.IsPermanent,
                input.ExpireDays,
                input.ForceReset,
                input.Title,
                input.MaxUseCount,
                input.PayloadJson);
        }
        else
        {
            Assert.If(!input.OwnerId.HasValue, "生成名片或群二维码必须指定 OwnerId");

            entity = await QrCodeManager.GenerateCardAsync(
                input.OwnerId!.Value,
                input.InviterId,
                input.IsPermanent,
                input.ExpireDays,
                input.ForceReset,
                input.Title,
                input.MaxUseCount,
                input.PayloadJson);
        }

        return await MapToGetOutputDtoAsync(entity);
    }

    /// <summary>
    /// 重置二维码（作废旧码并签发新码）
    /// </summary>
    [HttpPost]
    public virtual Task<QrCodeDetailDto> ResetAsync(QrCodeGenerateInput input)
    {
        input.ForceReset = true;
        return GenerateAsync(input);
    }

    /// <summary>
    /// 根据短码获取二维码摘要及名片详情
    /// </summary>
    [HttpGet]
    public virtual async Task<QrCodeScanDataInfo> GetByCodeAsync(string code, long? currentChatObjectId = null)
    {
        Assert.If(string.IsNullOrWhiteSpace(code), "二维码短码不能为空");

        var parsed = QrCodeTemplateMatcher.ParseQrContent(code, Options);
        var normalizedCode = parsed?.Kind == QrCodeParsedKind.CodeTicket && !string.IsNullOrWhiteSpace(parsed.Code)
            ? parsed.Code
            : code.Trim();

        return await QrCodeManager.ResolveByCodeAsync(normalizedCode, currentChatObjectId);
    }

    /// <summary>
    /// 解析完整二维码内容（支持配置模板、HTTPS短链及历史URI）
    /// </summary>
    [HttpPost]
    public virtual async Task<QrCodeScanDataInfo> ResolveAsync(string content, long? currentChatObjectId = null)
    {
        Assert.If(string.IsNullOrWhiteSpace(content), "二维码内容不能为空");
        return await QrCodeManager.ResolveScanAsync(content, currentChatObjectId);
    }

    /// <summary>
    /// 扫码执行加好友或入群操作
    /// </summary>
    [HttpPost]
    public virtual async Task<QrCodeApplyOutput> ApplyAsync(QrCodeApplyInput input)
    {
        var parsed = QrCodeTemplateMatcher.ParseQrContent(input.Code, Options);
        var code = parsed?.Kind == QrCodeParsedKind.CodeTicket && !string.IsNullOrWhiteSpace(parsed.Code)
            ? parsed.Code
            : input.Code?.Trim();

        var applicantId = input.ApplicantChatObjectId;
        if (!applicantId.HasValue)
        {
            var userId = CurrentUser.GetId();
            var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(userId);
            if (ownedIds == null || ownedIds.Count == 0)
            {
                throw new UserFriendlyException("当前用户没有可用的聊天身份");
            }
            applicantId = ownedIds[0];
        }

        var result = await QrCodeManager.ApplyAsync(code, applicantId.Value, input.RequestMessage);
        return ObjectMapper.Map<QrCodeApplyResult, QrCodeApplyOutput>(result);
    }

    /// <summary>
    /// 停用指定二维码
    /// </summary>
    [HttpPost]
    public virtual Task DisableAsync(Guid id)
    {
        return QrCodeManager.DisableAsync(id);
    }

    protected virtual async Task<bool> IsOwnedAnyAsync(params long?[] chatObjectIds)
    {
        if (!CurrentUser.Id.HasValue)
        {
            return false;
        }

        var candidateIds = chatObjectIds.Where(x => x.HasValue).Select(x => x!.Value).ToList();
        if (candidateIds.Count == 0)
        {
            return false;
        }

        var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
        return ownedIds != null && candidateIds.Any(id => ownedIds.Contains(id));
    }
}
