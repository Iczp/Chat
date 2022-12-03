using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ChatObjects
{
    public class ChatObjectManager : DomainService, IChatObjectManager
    {

        protected IReadOnlyRepository<ChatObject, Guid> ChatObjectReadOnlyRepository { get; }

        public ChatObjectManager(IReadOnlyRepository<ChatObject, Guid> chatObjectReadOnlyRepository)
        {
            ChatObjectReadOnlyRepository = chatObjectReadOnlyRepository;
        }

        public async Task<List<ChatObject>> GetListByUserId(Guid userId)
        {
            return await ChatObjectReadOnlyRepository.GetListAsync(x => x.AppUserId == userId);
        }

        public Task<ChatObject> GetAsync(Guid chatObjectId)
        {
            return ChatObjectReadOnlyRepository.GetAsync(chatObjectId);
        }
    }
}
