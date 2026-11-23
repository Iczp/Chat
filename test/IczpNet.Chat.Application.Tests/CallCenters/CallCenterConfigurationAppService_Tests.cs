using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.CallCenters;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Volo.Abp.Authorization;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Claims;
using Xunit;

namespace IczpNet.Chat;

public class CallCenterConfigurationAppService_Tests : ChatApplicationTestBase
{
    protected override void AfterAddApplication(IServiceCollection services)
    {
        var distributedLock = Substitute.For<IAbpDistributedLock>();
        distributedLock.TryAcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<IAbpDistributedLockHandle>());
        services.AddSingleton(distributedLock);
    }

    [Fact]
    public async Task Shop_owner_can_update_and_delete_configuration_but_other_user_cannot()
    {
        var ownerUser = Guid.NewGuid();
        var customerUser = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var queuedSessionId = Guid.NewGuid();
        var secondSessionId = Guid.NewGuid();
        long shopId = 0;
        await WithUnitOfWorkAsync(async () =>
        {
            var types = GetRequiredService<IRepository<ChatObjectType, string>>();
            var type = await types.FindAsync(ChatObjectTypeEnums.ShopKeeper.ToString())
                ?? await types.InsertAsync(new ChatObjectType(ChatObjectTypeEnums.ShopKeeper.ToString())
                { Name = "shop", ObjectType = ChatObjectTypeEnums.ShopKeeper }, autoSave: true);
            var shop = new ChatObject("shop", Guid.NewGuid().ToString("N"), type, null);
            shop.SetAppUserId(ownerUser);
            shopId = (await GetRequiredService<IRepository<ChatObject, long>>().InsertAsync(shop, autoSave: true)).Id;
            var customerType = await types.FindAsync(ChatObjectTypeEnums.Personal.ToString())
                ?? await types.InsertAsync(new ChatObjectType(ChatObjectTypeEnums.Personal.ToString())
                { Name = "personal", ObjectType = ChatObjectTypeEnums.Personal }, autoSave: true);
            var customer = new ChatObject("customer", Guid.NewGuid().ToString("N"), customerType, null);
            customer.SetAppUserId(customerUser);
            var customerId = (await GetRequiredService<IRepository<ChatObject, long>>()
                .InsertAsync(customer, autoSave: true)).Id;
            var queuedAt = DateTime.UtcNow;
            var assignment = new CallCenterAssignment(Guid.NewGuid(), shopId, sessionId, customerId, queuedAt);
            assignment.Assign(shopId);
            await GetRequiredService<IRepository<CallCenterAssignment, Guid>>().InsertAsync(assignment, autoSave: true);
            var second = new CallCenterAssignment(Guid.NewGuid(), shopId, secondSessionId, customerId, queuedAt);
            second.Assign(shopId);
            await GetRequiredService<IRepository<CallCenterAssignment, Guid>>().InsertAsync(second, autoSave: true);
            var queued = new CallCenterAssignment(Guid.NewGuid(), shopId, queuedSessionId, customerId,
                DateTime.UtcNow.AddMinutes(1));
            await GetRequiredService<IRepository<CallCenterAssignment, Guid>>().InsertAsync(queued, autoSave: true);
        });
        var principals = GetRequiredService<ICurrentPrincipalAccessor>();
        var service = GetRequiredService<ICallCenterAppService>();
        ClaimsPrincipal Principal(Guid id) => new(new ClaimsIdentity(
            [new Claim(AbpClaimTypes.UserId, id.ToString())], "test"));

        using (principals.Change(Principal(Guid.NewGuid())))
        {
            await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.GetConfigurationAsync(shopId));
            await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.GetAssignmentAsync(sessionId));
            await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.GetAssignmentsAsync(shopId,
                CallCenterAssignmentStatus.Assigned));
        }

        using (principals.Change(Principal(customerUser)))
        {
            Assert.Equal(shopId, (await service.GetAssignmentAsync(sessionId)).AgentId);
            Assert.Equal(1, (await service.GetAssignmentAsync(queuedSessionId)).QueuePosition);
        }

        using (principals.Change(Principal(ownerUser)))
        {
            var agents = await service.GetAgentsAsync(shopId);
            Assert.Contains(agents, x => x.Id == shopId && x.ActiveCount >= 0);
            var firstPage = await service.GetAssignmentsAsync(shopId, CallCenterAssignmentStatus.Assigned,
                maxResultCount: 1);
            Assert.Single(firstPage.Items);
            Assert.NotNull(firstPage.NextId);
            var secondPage = await service.GetAssignmentsAsync(shopId, CallCenterAssignmentStatus.Assigned,
                firstPage.NextQueuedAt, firstPage.NextId, maxResultCount: 1);
            Assert.Single(secondPage.Items);
            Assert.NotEqual(firstPage.Items[0].SessionId, secondPage.Items[0].SessionId);
            Assert.Single((await service.GetAssignmentsAsync(shopId, CallCenterAssignmentStatus.Queued)).Items);
            await service.CancelQueueAsync(queuedSessionId);
            Assert.Empty((await service.GetAssignmentsAsync(shopId, CallCenterAssignmentStatus.Queued)).Items);
            var defaults = await service.GetConfigurationAsync(shopId);
            Assert.Equal(CallCenterOverflowStrategy.AllowOverCapacity, defaults.OverflowStrategy);
            var configured = await service.UpdateConfigurationAsync(shopId, new UpdateCallCenterConfigurationInput
            {
                DistributionStrategy = CallCenterDistributionStrategy.LeastActive,
                OverflowStrategy = CallCenterOverflowStrategy.Queue,
                MaxActiveConversations = 3,
                ActiveTimeoutMinutes = 45,
                RequireOnline = true
            });
            Assert.Equal(3, configured.MaxActiveConversations);
            Assert.Equal(CallCenterOverflowStrategy.Queue, (await service.GetConfigurationAsync(shopId)).OverflowStrategy);
            await service.DeleteConfigurationAsync(shopId);
            Assert.Equal(CallCenterOverflowStrategy.AllowOverCapacity,
                (await service.GetConfigurationAsync(shopId)).OverflowStrategy);
            Assert.Equal(shopId, (await service.GetAssignmentAsync(sessionId)).AgentId);
        }
    }
}
