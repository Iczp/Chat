﻿using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace IczpNet.Chat.ChatObjectTypes;

public class ChatObjectTypeDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    protected ILogger<ChatObjectTypeDataSeedContributor> Logger { get; }
    protected IConfiguration Configuration { get; }
    protected ICurrentTenant CurrentTenant { get; }
    protected IRepository<ChatObjectType, string> Repository { get; }

    public ChatObjectTypeDataSeedContributor(
        IConfiguration configuration,
        ICurrentTenant currentTenant,
        IRepository<ChatObjectType, string> repository,
        ILogger<ChatObjectTypeDataSeedContributor> logger)
    {
        Configuration = configuration;
        CurrentTenant = currentTenant;
        Repository = repository;
        Logger = logger;
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
        foreach (ChatObjectTypeEnums value in Enum.GetValues(typeof(ChatObjectTypeEnums)))
        {
            var id = value.ToString();
            var isAny = await Repository.AnyAsync(x => x.Id.Equals(id));
            if (isAny)
            {
                Logger.LogInformation($"Already exists:{id}");
                continue;
            }
            await Repository.InsertAsync(new ChatObjectType(id)
            {
                Name = value.GetDescription(),
                ObjectType = value,
                IsStatic = true,
            });
            Logger.LogInformation($"Add {nameof(ChatObjectType)}:{id}");
        }
    }
}
