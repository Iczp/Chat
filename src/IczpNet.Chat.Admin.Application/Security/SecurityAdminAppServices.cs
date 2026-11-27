using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.Admin.Security.Dtos;
using IczpNet.Chat.Security;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.Security;

/// <summary>
/// URL 风控规则管理服务
/// </summary>
[Authorize(ChatAdminPermissions.UrlSecurityRules.Default)]
[ExposeServices(typeof(UrlSecurityRuleAppService), typeof(IUrlSecurityRuleAdminAppService))]
public class UrlSecurityRuleAppService(IUrlSecurityAdminManager urlSecurityManager)
    : ApplicationService, IUrlSecurityRuleAdminAppService
{
    protected IUrlSecurityAdminManager UrlSecurityManager { get; } = urlSecurityManager;

    [Authorize(ChatAdminPermissions.UrlSecurityRules.GetList)]
    public virtual async Task<PagedResultDto<UrlSecurityRuleAdminDto>> GetListAsync(
        UrlSecurityRuleAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new UrlSecurityRuleAdminGetListInput();

        var result = await UrlSecurityManager.GetRulesAdminListAsync(
            input.Keyword,
            input.Level,
            input.Category,
            input.IsEnabled,
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            cancellationToken);

        return new PagedResultDto<UrlSecurityRuleAdminDto>(
            result.TotalCount,
            result.Items.Select(MapToRuleDto).ToList());
    }

    [Authorize(ChatAdminPermissions.UrlSecurityRules.GetDetail)]
    public virtual async Task<UrlSecurityRuleAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await UrlSecurityManager.GetRuleAdminDetailAsync(id, cancellationToken);
        return MapToRuleDto(entity);
    }

    [Authorize(ChatAdminPermissions.UrlSecurityRules.Create)]
    public virtual async Task<UrlSecurityRuleAdminDto> CreateAsync(
        UrlSecurityRuleAdminCreateInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await UrlSecurityManager.CreateRuleAdminAsync(
            input.Host,
            input.Level,
            input.Category,
            input.Reason,
            input.IsEnabled,
            input.ExpireTime,
            cancellationToken);
        return MapToRuleDto(entity);
    }

    [Authorize(ChatAdminPermissions.UrlSecurityRules.Update)]
    public virtual async Task<UrlSecurityRuleAdminDto> UpdateAsync(
        Guid id,
        UrlSecurityRuleAdminUpdateInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await UrlSecurityManager.UpdateRuleAdminAsync(
            id,
            input.Host,
            input.Level,
            input.Category,
            input.Reason,
            input.IsEnabled,
            input.ExpireTime,
            cancellationToken);
        return MapToRuleDto(entity);
    }

    [Authorize(ChatAdminPermissions.UrlSecurityRules.SetEnabled)]
    public virtual async Task<UrlSecurityRuleAdminDto> SetEnabledAsync(
        Guid id,
        UrlSecurityRuleAdminSetEnabledInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await UrlSecurityManager.SetRuleEnabledAdminAsync(id, input.IsEnabled, cancellationToken);
        return MapToRuleDto(entity);
    }

    [Authorize(ChatAdminPermissions.UrlSecurityRules.Delete)]
    public virtual Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return UrlSecurityManager.DeleteRuleAdminAsync(id, cancellationToken);
    }

    [Authorize(ChatAdminPermissions.UrlSecurityRules.RefreshCache)]
    public virtual Task RefreshCacheAsync()
    {
        return UrlSecurityManager.RefreshCacheAsync();
    }

    [Authorize(ChatAdminPermissions.UrlSecurityRules.CheckUrl)]
    public virtual async Task<UrlSecurityAdminCheckOutput> CheckUrlAsync(UrlSecurityAdminCheckInput input)
    {
        var result = await UrlSecurityManager.CheckUrlAsync(input.Url);
        return new UrlSecurityAdminCheckOutput
        {
            Url = result.Url,
            Domain = result.Domain,
            Level = result.Level,
            Category = result.Category,
            Message = result.Message,
            InterceptReason = result.InterceptReason,
            Source = result.Source
        };
    }

    internal static UrlSecurityRuleAdminDto MapToRuleDto(UrlSecurityRule entity)
    {
        return new UrlSecurityRuleAdminDto
        {
            Id = entity.Id,
            Host = entity.Host,
            Level = entity.Level,
            Category = entity.Category,
            Reason = entity.Reason,
            IsEnabled = entity.IsEnabled,
            ExpireTime = entity.ExpireTime,
            HitCount = entity.HitCount,
            CreationTime = entity.CreationTime,
            LastModificationTime = entity.LastModificationTime
        };
    }
}

/// <summary>
/// 违规举报工单管理服务
/// </summary>
[Authorize(ChatAdminPermissions.SecurityReports.Default)]
[ExposeServices(typeof(SecurityReportAppService), typeof(ISecurityReportAdminAppService))]
public class SecurityReportAppService(IUrlSecurityAdminManager urlSecurityManager)
    : ApplicationService, ISecurityReportAdminAppService
{
    protected IUrlSecurityAdminManager UrlSecurityManager { get; } = urlSecurityManager;

    [Authorize(ChatAdminPermissions.SecurityReports.GetList)]
    public virtual async Task<PagedResultDto<SecurityReportAdminDto>> GetListAsync(
        SecurityReportAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new SecurityReportAdminGetListInput();

        var result = await UrlSecurityManager.GetReportsAdminListAsync(
            input.Keyword,
            input.Status,
            input.TargetType,
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            cancellationToken);

        return new PagedResultDto<SecurityReportAdminDto>(
            result.TotalCount,
            result.Items.Select(MapToReportDto).ToList());
    }

    [Authorize(ChatAdminPermissions.SecurityReports.GetDetail)]
    public virtual async Task<SecurityReportAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await UrlSecurityManager.GetReportAdminDetailAsync(id, cancellationToken);
        return MapToReportDto(entity);
    }

    [Authorize(ChatAdminPermissions.SecurityReports.Handle)]
    public virtual async Task<SecurityReportAdminDto> HandleAsync(
        Guid id,
        SecurityReportAdminHandleInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await UrlSecurityManager.HandleReportAdminAsync(
            id,
            input.Status,
            input.AuditRemark,
            input.AddBlacklist,
            cancellationToken);
        return MapToReportDto(entity);
    }

    private static SecurityReportAdminDto MapToReportDto(SecurityReport entity)
    {
        return new SecurityReportAdminDto
        {
            Id = entity.Id,
            ReporterChatObjectId = entity.ReporterChatObjectId,
            ReporterSessionUnitId = entity.ReporterSessionUnitId,
            TargetType = entity.TargetType,
            TargetContent = entity.TargetContent,
            MessageId = entity.MessageId,
            Reason = entity.Reason,
            Description = entity.Description,
            Status = entity.Status,
            AuditRemark = entity.AuditRemark,
            CreationTime = entity.CreationTime,
            LastModificationTime = entity.LastModificationTime
        };
    }
}
