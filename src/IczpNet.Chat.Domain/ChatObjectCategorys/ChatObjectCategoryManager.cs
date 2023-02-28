using IczpNet.AbpTrees;
using System;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.ChatObjectCategorys
{
    public class ChatObjectCategoryManager : TreeManager<ChatObjectCategory, Guid, ChatObjectCategoryInfo>, IChatObjectCategoryManager  
    {
        public ChatObjectCategoryManager(IRepository<ChatObjectCategory, Guid> repository) : base(repository)
        {
        }
    }
}
