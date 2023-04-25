using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace Rctea.IM.GlobalPermissions;

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

        var list = new List<SessionPermissionDefinition>();

        foreach (var id in createIdList)
        {
            list.Add(new SessionPermissionDefinition(id)
            {
                Name = id.MaxLength(50),
                Description = $"Initialization：{_click.Now}",
            });
        }
        await _sessionPermissionDefinitionRepository.InsertManyAsync(list);

        _logger.LogInformation($"Added {createIdList.Count()} permissions:{string.Join(",", createIdList.ToArray())}");
    }
}

