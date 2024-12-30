using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Volo.Abp.Guids;

namespace IczpNet.Chat.EntryNames;

public class EntryNameDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    protected ILogger<EntryNameDataSeedContributor> Logger { get; }
    protected IConfiguration Configuration { get; }
    protected ICurrentTenant CurrentTenant { get; }

    protected IRepository<EntryName, Guid> Repository { get; }
    protected IGuidGenerator GuidGenerator { get; }

    public EntryNameDataSeedContributor(
        IConfiguration configuration,
        ICurrentTenant currentTenant,
        IRepository<EntryName, Guid> repository,
        ILogger<EntryNameDataSeedContributor> logger,
        IGuidGenerator guidGenerator)
    {
        Configuration = configuration;
        CurrentTenant = currentTenant;
        Repository = repository;
        Logger = logger;
        GuidGenerator = guidGenerator;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        using (CurrentTenant.Change(context?.TenantId))
        {
            await CreateAsync();
        }
    }

    private async Task CreateAsync()
    {
        foreach (var name in EntryNameConsts.GetAll())
        {
            if (await Repository.AnyAsync(x => x.Code == name))
            {
                continue;
            }
            var entity = await Repository.InsertAsync(new EntryName(GuidGenerator.Create(), name, null)
            {
                //Name = name,
                Code = name,
                IsStatic = true,
                IsPublic = true,
            });
            Logger.LogInformation($"Add {nameof(EntryName)},id={entity.Id},Code ={entity.Name}");
        }
    }
}
