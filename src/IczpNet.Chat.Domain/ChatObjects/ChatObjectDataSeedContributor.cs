using IczpNet.Chat.ChatObjectTypes;
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

namespace IczpNet.Chat.ChatObjects
{
    public class ChatObjectDataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        protected ILogger<ChatObjectDataSeedContributor> Logger { get; }
        protected IConfiguration Configuration { get; }
        protected ICurrentTenant CurrentTenant { get; }
        protected IRepository<ChatObject, Guid> Repository { get; }
        protected IChatObjectTypeManager ChatObjectTypeManager { get; }
        public ChatObjectDataSeedContributor(
            IConfiguration configuration,
            ICurrentTenant currentTenant,
            IRepository<ChatObject, Guid> repository,
            ILogger<ChatObjectDataSeedContributor> logger,
            IChatObjectTypeManager chatObjectTypeManager)
        {
            Configuration = configuration;
            CurrentTenant = currentTenant;
            Repository = repository;
            Logger = logger;
            ChatObjectTypeManager = chatObjectTypeManager;
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
            if(!await Repository.AnyAsync(x=>x.Code== "GroupAssistant"))
            {
                var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.Robot);
                var robot = new ChatObject(Guid.Parse("99999999-8888-7777-6666-000000000001"), "群助手", chatObjectType, null)
                {
                    Code = "GroupAssistant",
                    Description = "我是机器人：加群"
                };
                await Repository.InsertAsync(robot);
            }
        }
    }
}
