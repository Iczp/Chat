using System;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Uow;

namespace IczpNet.Chat.ShopWaiters;

public class ShopWaiterSessionSyncJobArgs
{
    public long ShopKeeperId { get; set; }
    public long WaiterId { get; set; }
}

/// <summary>Backfills a new shop waiter's shared customer sessions in bounded batches.</summary>
public class ShopWaiterSessionSyncJob(
    IRepository<SessionUnit, Guid> units,
    IRepository<Session, Guid> sessions,
    IRepository<ChatObject, long> objects,
    ISessionUnitManager unitManager,
    IAsyncQueryableExecuter executer,
    IUnitOfWorkManager uowManager) : AsyncBackgroundJob<ShopWaiterSessionSyncJobArgs>, ITransientDependency
{
    public override async Task ExecuteAsync(ShopWaiterSessionSyncJobArgs args)
    {
        Guid? cursor = null;
        while (true)
        {
            using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);
            var waiter = await objects.GetAsync(args.WaiterId);
            if (waiter.ObjectType != ChatObjectTypeEnums.ShopWaiter || waiter.ParentId != args.ShopKeeperId)
                throw new InvalidOperationException("The waiter no longer belongs to the shop.");
            var query = (await units.GetQueryableAsync())
                .Where(x => x.OwnerId == args.ShopKeeperId && x.SessionId.HasValue && x.DestinationId.HasValue)
                .Where(x => !cursor.HasValue || x.Id.CompareTo(cursor.Value) > 0)
                .OrderBy(x => x.Id)
                .Select(x => new { x.Id, SessionId = x.SessionId.Value, CustomerId = x.DestinationId.Value })
                .Take(200);
            var batch = await executer.ToListAsync(query);
            if (batch.Count == 0)
            {
                await uow.CompleteAsync();
                return;
            }
            var sessionIds = batch.Select(x => x.SessionId).Distinct().ToArray();
            var customerIds = batch.Select(x => x.CustomerId).Distinct().ToArray();
            var sessionMap = (await sessions.GetListAsync(x => sessionIds.Contains(x.Id))).ToDictionary(x => x.Id);
            var customers = (await objects.GetListAsync(x => customerIds.Contains(x.Id)))
                .Where(x => x.ObjectType is ChatObjectTypeEnums.Personal or ChatObjectTypeEnums.Anonymous or ChatObjectTypeEnums.Customer)
                .ToDictionary(x => x.Id);
            foreach (var item in batch)
                if (sessionMap.TryGetValue(item.SessionId, out var session) && customers.TryGetValue(item.CustomerId, out var customer))
                    await unitManager.CreateIfNotContainsAsync(session, waiter, customer, x => x.JoinWay = JoinWays.AutoJoin);
            cursor = batch[^1].Id;
            await uow.CompleteAsync();
            if (batch.Count < 200) return;
        }
    }
}
