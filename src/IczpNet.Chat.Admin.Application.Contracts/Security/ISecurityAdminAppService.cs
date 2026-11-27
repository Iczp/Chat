using System;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.Security.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.Security;

public interface IUrlSecurityRuleAdminAppService : IApplicationService
{
    Task<PagedResultDto<UrlSecurityRuleAdminDto>> GetListAsync(
        UrlSecurityRuleAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<UrlSecurityRuleAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<UrlSecurityRuleAdminDto> CreateAsync(
        UrlSecurityRuleAdminCreateInput input,
        CancellationToken cancellationToken = default);

    Task<UrlSecurityRuleAdminDto> UpdateAsync(
        Guid id,
        UrlSecurityRuleAdminUpdateInput input,
        CancellationToken cancellationToken = default);

    Task<UrlSecurityRuleAdminDto> SetEnabledAsync(
        Guid id,
        UrlSecurityRuleAdminSetEnabledInput input,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task RefreshCacheAsync();

    Task<UrlSecurityAdminCheckOutput> CheckUrlAsync(UrlSecurityAdminCheckInput input);
}

public interface ISecurityReportAdminAppService : IApplicationService
{
    Task<PagedResultDto<SecurityReportAdminDto>> GetListAsync(
        SecurityReportAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<SecurityReportAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SecurityReportAdminDto> HandleAsync(
        Guid id,
        SecurityReportAdminHandleInput input,
        CancellationToken cancellationToken = default);
}
