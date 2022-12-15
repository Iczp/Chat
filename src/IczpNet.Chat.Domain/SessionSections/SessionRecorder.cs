using IczpNet.Chat.ChatObjects;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections
{
    public class SessionRecorder : DomainService, ISessionRecorder
    {
        protected IRepository<ChatObject, Guid> Repository { get; }

        public SessionRecorder(IRepository<ChatObject, Guid> repository)
        {
            Repository = repository;
        }

        public async Task<long> GetMaxIdAsync(Guid chatObjectId)
        {
            return (await Repository.GetQueryableAsync()).Where(x => x.Id.Equals(chatObjectId)).Select(x => x.MaxMessageAutoId).FirstOrDefault();
        }

        public Task<long> GetMaxIdAsync(ChatObject chatObject)
        {
            return Task.FromResult(chatObject.MaxMessageAutoId);
        }

        public async Task<long> UpdateMaxIdAsync(Guid chatObjectId, long maxMessageAutoId, bool isForce = false)
        {
            var chatObject = await Repository.GetAsync(chatObjectId);
            return await UpdateMaxIdAsync(chatObject, maxMessageAutoId, isForce);
        }

        public async Task<long> UpdateMaxIdAsync(ChatObject chatObject, long maxMessageAutoId, bool isForce = false)
        {
            if (isForce || maxMessageAutoId > chatObject.MaxMessageAutoId)
            {
                chatObject.SetMaxMessageAutoId(maxMessageAutoId);
                await Repository.UpdateAsync(chatObject, true);
            }
            return chatObject.MaxMessageAutoId;
        }
    }
}
