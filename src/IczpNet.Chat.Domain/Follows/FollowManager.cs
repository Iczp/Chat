using IczpNet.AbpCommons;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Follows
{
    public class FollowManager : DomainService, IFollowManager
    {
        protected ISessionUnitManager SessionUnitManager { get; }

        protected IUnitOfWorkManager UnitOfWorkManager { get; }

        protected IRepository<Follow> Repository { get; }

        public FollowManager(ISessionUnitManager sessionUnitManager,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<Follow> repository)
        {
            SessionUnitManager = sessionUnitManager;
            UnitOfWorkManager = unitOfWorkManager;
            Repository = repository;
        }

        public async Task<bool> CreateAsync(Guid ownerId, List<Guid> idList)
        {
            var owner = await SessionUnitManager.GetAsync(ownerId);

            return await CreateAsync(owner, idList);
        }

        public async Task<bool> CreateAsync(SessionUnit owner, List<Guid> idList)
        {
            var destinationList = await SessionUnitManager.GetManyAsync(idList);

            foreach (var item in destinationList)
            {
                Assert.If(owner.SessionId != item.SessionId, $"Not in the same session,id:{item.Id}");
            }

            var followedIdList = (await Repository.GetQueryableAsync())
                 .Where(x => x.OwnerId == owner.Id)
                 .Select(x => x.DestinationId)
                 .ToList();

            var newList = idList.Except(followedIdList)
                .Select(x => new Follow(owner, x))
                .ToList();

            if (newList.Any())
            {
                await Repository.InsertManyAsync(newList, autoSave: true);
            }

            return true;
        }

        public async Task DeleteAsync(Guid ownerId, List<Guid> idList)
        {
            await Repository.DeleteAsync(x => x.OwnerId == ownerId && idList.Contains(x.DestinationId));
        }

        public async Task DeleteAsync(SessionUnit owner, List<Guid> idList)
        {
            await DeleteAsync(owner.Id, idList);
        }
    }
}
