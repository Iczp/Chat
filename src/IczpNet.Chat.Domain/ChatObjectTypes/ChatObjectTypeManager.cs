using IczpNet.Chat.Enums;
using System;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ChatObjectTypes
{
    public class ChatObjectTypeManager : DomainService, IChatObjectTypeManager
    {
        protected IRepository<ChatObjectType, string> Repository { get; }

        protected IDistributedCache<ChatObjectTypeInfo, Guid> ChatObjectTypeCache { get; }

        public ChatObjectTypeManager(
            IRepository<ChatObjectType, string> repository)
        {
            Repository = repository;
        }

        public virtual Task<ChatObjectType> GetAsync(string id)
        {
            return Repository.GetAsync(id);
        }

        public virtual Task<ChatObjectType> GetAsync(ChatObjectTypeEnums chatObjectTypeEnum)
        {
            return GetAsync(chatObjectTypeEnum.ToString());
        }
    }
}
