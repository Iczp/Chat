using IczpNet.Chat.ChatObjectTypes;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using IczpNet.AbpCommons;
using IczpNet.Chat.Management.ChatObjectTypes.Dtos;
using IczpNet.Chat.Management.ChatObjectTypes;
using IczpNet.Chat.Management.BaseAppServices;

namespace IczpNet.Chat.Management.ChatObjectTypeServices
{
    public class ChatObjectTypeManagementAppService
        : CrudChatManagementAppService<
            ChatObjectType,
            ChatObjectTypeDetailDto,
            ChatObjectTypeDto,
            string,
            ChatObjectTypeGetListInput,
            ChatObjectTypeCreateInput,
            ChatObjectTypeUpdateInput>,
        IChatObjectTypeManagementAppService
    {
        public ChatObjectTypeManagementAppService(IRepository<ChatObjectType, string> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<ChatObjectType>> CreateFilteredQueryAsync(ChatObjectTypeGetListInput input)
        {
            return (await ReadOnlyRepository.GetQueryableAsync())
                .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Name.Contains(input.Keyword));
        }


        protected override async Task CheckCreateAsync(ChatObjectTypeCreateInput input)
        {
            Assert.If(await Repository.AnyAsync(x => x.Name.Equals(input.Name)), $"Already exists [{typeof(ChatObjectType)}] name:{input.Name}");
            await base.CheckCreateAsync(input);
        }

        protected override async Task CheckUpdateAsync(string id, ChatObjectType entity, ChatObjectTypeUpdateInput input)
        {
            Assert.If(await Repository.AnyAsync(x => x.Id.Equals(id) && x.Name.Equals(input.Name)), $"Already exists [{typeof(ChatObjectType)}] name:{input.Name}");
            await base.CheckUpdateAsync(id, entity, input);
        }
    }
}
