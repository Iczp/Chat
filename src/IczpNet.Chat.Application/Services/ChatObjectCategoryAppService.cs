using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjectCategorys.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.ChatObjectCategorys
{
    public class ChatObjectCategoryAppService
        : CrudTreeChatAppService<
            ChatObjectCategory,
            Guid,
            ChatObjectCategoryDetailDto,
            ChatObjectCategoryDto,
            ChatObjectCategoryGetListInput,
            ChatObjectCategoryCreateInput,
            ChatObjectCategoryUpdateInput,
            ChatObjectCategoryInfo>,
        IChatObjectCategoryAppService
    {
        public ChatObjectCategoryAppService(IRepository<ChatObjectCategory, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<ChatObjectCategory>> CreateFilteredQueryAsync(ChatObjectCategoryGetListInput input)
        {

            Assert.If(!input.IsEnabledParentId && input.ParentId.HasValue, "When [IsEnabledParentId]=false,then [ParentId] != null");

            return (await Repository.GetQueryableAsync())
                //.WhereIf(input.Depth.HasValue, (x) => x.Depth == input.Depth)
                .WhereIf(!input.ChatObjectTypeId.IsNullOrWhiteSpace(), (x) => x.ChatObjectTypeId == input.ChatObjectTypeId)
                .WhereIf(input.IsEnabledParentId, (x) => x.ParentId == input.ParentId)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.Contains(input.Keyword));


        }
    }
}
