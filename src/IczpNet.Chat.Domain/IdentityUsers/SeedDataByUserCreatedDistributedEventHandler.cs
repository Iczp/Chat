using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Json;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace IczpNet.Chat.IdentityUsers;

public class SeedDataByUserCreatedDistributedEventHandler(
    ISessionUnitManager sessionUnitManager,
    IAbpDistributedLock abpDistributedLock,
    IJsonSerializer jsonSerializer,
    IUnitOfWorkManager unitOfWorkManager,
    IChatObjectManager chatObjectManager) : DomainService,
    IDistributedEventHandler<EntityCreatedEto<UserEto>>,

    ITransientDependency
{
    public ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    public IAbpDistributedLock AbpDistributedLock { get; } = abpDistributedLock;
    public IJsonSerializer JsonSerializer { get; } = jsonSerializer;
    public IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    public IChatObjectManager ChatObjectManager { get; } = chatObjectManager;

    public IUnitOfWork Begin(AbpUnitOfWorkOptions options, bool requiresNew = false)
    {
        throw new System.NotImplementedException();
    }

    public void BeginReserved(string reservationName, AbpUnitOfWorkOptions options)
    {
        throw new System.NotImplementedException();
    }

    public async Task HandleEventAsync(EntityCreatedEto<UserEto> eventData)
    {
        var userInfo = eventData.Entity;

        Logger.LogInformation($"{nameof(SeedDataByUserCreatedDistributedEventHandler)} eventData: {JsonSerializer.Serialize(eventData.Entity)}");

        await using var handle = await AbpDistributedLock.TryAcquireAsync(nameof(SeedDataByUserCreatedDistributedEventHandler));

        if (handle == null)
        {
            return;
        }

        // 分布式事件要开启工作单元
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: false);

        //...
        //create chat object
       var userChatObject =  await ChatObjectManager.GenerateByUserCreatedAsync(userInfo);

        //create session unit
        await SessionUnitManager.GenerateDefaultSessionByChatObjectAsync(userChatObject);

        await uow.CompleteAsync();
    }
}
