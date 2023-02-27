using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace IczpNet.Chat.ChatObjects
{
    public class ChatObjectDataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        protected ILogger<ChatObjectDataSeedContributor> Logger { get; }
        protected IConfiguration Configuration { get; }
        protected ICurrentTenant CurrentTenant { get; }
        protected IRepository<ChatObject, Guid> Repository { get; }

        public ChatObjectDataSeedContributor(
            IConfiguration configuration,
            ICurrentTenant currentTenant,
            IRepository<ChatObject, Guid> repository,
            ILogger<ChatObjectDataSeedContributor> logger)
        {
            Configuration = configuration;
            CurrentTenant = currentTenant;
            Repository = repository;
            Logger = logger;
        }

        [UnitOfWork]
        public async Task SeedAsync(DataSeedContext context)
        {
            using (CurrentTenant.Change(context?.TenantId))
            {
                await CreateAsync();
            }
        }

        private async Task CreateAsync()
        {
            await Task.CompletedTask;
        }
    }
}
