using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.ChatObjectTypes.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using IczpNet.AbpCommons;
using IczpNet.Chat.Permissions;

namespace IczpNet.Chat.ChatObjectTypeServices
{
    public class ChatObjectTypeAppService
        : CrudChatAppService<
            ChatObjectType,
            ChatObjectTypeDetailDto,
            ChatObjectTypeDto,
            string,
            ChatObjectTypeGetListInput,
            ChatObjectTypeCreateInput,
            ChatObjectTypeUpdateInput>,
        IChatObjectTypeAppService
    {

        protected override string GetPolicyName { get; set; } = ChatPermissions.ChatObjectTypePermission.Default;
        protected override string GetListPolicyName { get; set; } = ChatPermissions.ChatObjectTypePermission.Default;
        protected override string CreatePolicyName { get; set; } = ChatPermissions.ChatObjectTypePermission.Create;
        protected override string UpdatePolicyName { get; set; } = ChatPermissions.ChatObjectTypePermission.Update;
        protected override string DeletePolicyName { get; set; } = ChatPermissions.ChatObjectTypePermission.Delete;

        public ChatObjectTypeAppService(
            IRepository<ChatObjectType, string> repository) : base(repository)
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
