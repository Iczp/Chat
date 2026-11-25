using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.Permissions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionPermissionDefinitions;

public class SessionPermissionDefinitionDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IConfiguration _configuration;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<SessionPermissionDefinitionDataSeedContributor> _logger;

    private readonly IClock _click;

    private readonly ISessionPermissionDefinitionRepository _sessionPermissionDefinitionRepository;
    public SessionPermissionDefinitionDataSeedContributor(
        IConfiguration configuration,
        ICurrentTenant currentTenant,
        ILogger<SessionPermissionDefinitionDataSeedContributor> logger,
        ISessionPermissionDefinitionRepository sessionPermissionDefinitionRepository,
        IClock click)
    {
        _configuration = configuration;
        _currentTenant = currentTenant;
        _logger = logger;
        _sessionPermissionDefinitionRepository = sessionPermissionDefinitionRepository;
        _click = click;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        {
            await CreateSessionPermissionDefinitionAsync();
        }
    }

    private async Task CreateSessionPermissionDefinitionAsync()
    {
        var inputIdList = SessionPermissionDefinitionConsts.GetAll().ToList();

        var sessionPermissionDefinition = await _sessionPermissionDefinitionRepository.GetListAsync(x => inputIdList.Contains(x.Id));

        var dbIdList = sessionPermissionDefinition.Select(x => x.Id).ToList();

        var createIdList = inputIdList.Except(dbIdList);

        if (!createIdList.Any())
        {
            _logger.LogInformation("No new session permissions added");
            return;
        }

        var displayNames = new Dictionary<string, string>
        {
            [SessionPermissionDefinitionConsts.AnnouncementPermission.Create] = "创建/提审群公告",
            [SessionPermissionDefinitionConsts.AnnouncementPermission.Publish] = "直接发布群公告",
            [SessionPermissionDefinitionConsts.AnnouncementPermission.Approve] = "审核群公告",
            [SessionPermissionDefinitionConsts.AnnouncementPermission.Update] = "管理/编辑群公告",
            [SessionPermissionDefinitionConsts.AnnouncementPermission.UpdateOwn] = "编辑/撤回本人公告",
            [SessionPermissionDefinitionConsts.AnnouncementPermission.UpdateAny] = "编辑/撤回他人公告",
            [SessionPermissionDefinitionConsts.AnnouncementPermission.Delete] = "删除群公告",
            [SessionPermissionDefinitionConsts.AnnouncementPermission.DeleteOwn] = "删除本人公告",
            [SessionPermissionDefinitionConsts.AnnouncementPermission.DeleteAny] = "删除他人公告",
            [SessionPermissionDefinitionConsts.AnnouncementPermission.Pin] = "置顶/调权群公告",
            [SessionPermissionDefinitionConsts.AnnouncementPermission.Remind] = "催办未确认成员",
            [SessionPermissionDefinitionConsts.AnnouncementPermission.ViewStat] = "查看公告回执统计",
        };

        var list = new List<SessionPermissionDefinition>();

        foreach (var id in createIdList)
        {
            var isAnnouncementPerm = id.StartsWith(SessionPermissionDefinitionConsts.AnnouncementPermission.Default);
            list.Add(new SessionPermissionDefinition(id)
            {
                Name = displayNames.TryGetValue(id, out var label) ? label : id.MaxLength(120),
                Description = $"Initialization：{_click.Now}",
                IsEnabled = isAnnouncementPerm,
            });
        }
        await _sessionPermissionDefinitionRepository.InsertManyAsync(list);

        _logger.LogInformation($"Added {createIdList.Count()} permissions:{string.Join(",", createIdList.ToArray())}");
    }
}

