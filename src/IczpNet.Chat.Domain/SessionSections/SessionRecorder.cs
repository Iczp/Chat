using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.ReadedRecorders;
using System;
using System.Collections.Generic;
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
        protected IRepository<ReadedRecorder, Guid> ReadedRecorderRepository { get; }

        public SessionRecorder(
            IRepository<ChatObject, Guid> repository,
            IRepository<ReadedRecorder, Guid> readedRecorderRepository)
        {
            Repository = repository;
            ReadedRecorderRepository = readedRecorderRepository;
        }

        public async Task<long> GetAsync(Guid ownerId)
        {
            return (await Repository.GetQueryableAsync()).Where(x => x.Id.Equals(ownerId)).Select(x => x.MaxMessageAutoId).FirstOrDefault();
        }

        public Task<long> GetAsync(ChatObject owner)
        {
            return Task.FromResult(owner.MaxMessageAutoId);
        }

        public async Task<long> UpdateAsync(Guid ownerId, long maxMessageAutoId, bool isForce = false)
        {
            var chatObject = await Repository.GetAsync(ownerId);
            return await UpdateAsync(chatObject, maxMessageAutoId, isForce);
        }

        public async Task<long> UpdateAsync(ChatObject owner, long maxMessageAutoId, bool isForce = false)
        {
            if (isForce || maxMessageAutoId > owner.MaxMessageAutoId)
            {
                owner.SetMaxMessageAutoId(maxMessageAutoId);
                await Repository.UpdateAsync(owner, true);
            }
            return owner.MaxMessageAutoId;
        }

        public Task<List<ReadedRecorder>> GetReadedsAsync(Guid ownerId)
        {
            return ReadedRecorderRepository.GetListAsync(x => x.OwnerId == ownerId);
        }
    }
}
