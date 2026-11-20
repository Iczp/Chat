using System;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Sessions;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace IczpNet.Chat;

public class ShopSessionGenerator_Tests : ChatApplicationTestBase
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Shop_and_customer_share_one_session_in_either_direction(int scenario)
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var types = GetRequiredService<IRepository<ChatObjectType, string>>();
            async Task<ChatObjectType> TypeAsync(ChatObjectTypeEnums value)
            {
                var id = value.ToString();
                return await types.FindAsync(id) ?? await types.InsertAsync(new ChatObjectType(id)
                {
                    Name = id,
                    ObjectType = value
                }, autoSave: true);
            }

            var objects = GetRequiredService<IRepository<ChatObject, long>>();
            var customer = await objects.InsertAsync(new ChatObject("customer", Guid.NewGuid().ToString("N"), await TypeAsync(ChatObjectTypeEnums.Personal), null), autoSave: true);
            var shop = await objects.InsertAsync(new ChatObject("shop", Guid.NewGuid().ToString("N"), await TypeAsync(ChatObjectTypeEnums.ShopKeeper), null), autoSave: true);
            var waiter = await objects.InsertAsync(new ChatObject("waiter", Guid.NewGuid().ToString("N"), await TypeAsync(ChatObjectTypeEnums.ShopWaiter), shop.Id), autoSave: true);
            var generator = GetRequiredService<ISessionGenerator>();

            var session = scenario switch
            {
                0 => await generator.MakeAsync(customer, shop),
                1 => await generator.MakeAsync(shop, customer),
                2 => await generator.MakeAsync(customer, waiter),
                _ => await generator.MakeAsync(waiter, customer)
            };

            Assert.Equal(3, session.UnitList.Count);
            Assert.Contains(session.UnitList, x => x.OwnerId == customer.Id && x.DestinationId == shop.Id);
            Assert.Contains(session.UnitList, x => x.OwnerId == shop.Id && x.DestinationId == customer.Id);
            Assert.Contains(session.UnitList, x => x.OwnerId == waiter.Id && x.DestinationId == customer.Id);
            Assert.Equal(session.Id, (await generator.MakeAsync(customer, waiter)).Id);
        });
    }
}
