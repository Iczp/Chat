using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;
using System.Security.Cryptography;
using IczpNet.Chat.SessionUnits;

namespace IczpNet.Chat.Sessions;

public class SessionGenerator : DomainService, ISessionGenerator
{
    protected IMessageRepository MessageRepository => LazyServiceProvider.LazyGetRequiredService<IMessageRepository>();
    protected IRepository<Session, Guid> SessionRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<Session, Guid>>();
    protected IChannelResolver ChannelResolver => LazyServiceProvider.LazyGetRequiredService<IChannelResolver>();
    protected ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();
    protected ChatObjectManager ChatObjectManager => LazyServiceProvider.LazyGetRequiredService<ChatObjectManager>();
    public SessionGenerator() { }

    protected virtual Task<Guid> MakeSesssionIdAsync(string input)
    {
        var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(input));

        string hashString = string.Join(string.Empty, hashBytes.Select(x => x.ToString("X2")));

        return Task.FromResult(new Guid(hashString));
    }

    protected virtual bool IsObjectType(ChatObjectTypeEnums input, params ChatObjectTypeEnums[] objectTypes)
    {
        return objectTypes.Contains(input);
    }

    private async Task ResolveShopWaiterId(ChatObject sender, ChatObject receiver, Func<long, Task> matchAction)
    {
        if (IsObjectType(receiver.ObjectType.Value, ChatObjectTypeEnums.ShopWaiter))
        {
            var list = new List<ChatObjectTypeEnums>() {
                ChatObjectTypeEnums.Personal,
                ChatObjectTypeEnums.Anonymous,
                ChatObjectTypeEnums.Customer,
            };
            if (receiver.ParentId.HasValue && list.Contains(sender.ObjectType.Value))
            {
                await matchAction?.Invoke(receiver.ParentId.Value);
            }
        }
    }

    private async Task ResolveSenderIsRobot(ChatObject sender, Func<Task> matchAction)
    {
        if (IsObjectType(sender.ObjectType.Value, ChatObjectTypeEnums.Robot))
        {
            await matchAction?.Invoke();
        }
    }

    protected virtual async Task<string> MakeSesssionKeyAsync(ChatObject sender, ChatObject receiver)
    {
        await Task.Yield();

        var senderId = sender.Id;

        var receiverId = receiver.Id;

        if (IsObjectType(receiver.ObjectType.Value, ChatObjectTypeEnums.Room, ChatObjectTypeEnums.Square, ChatObjectTypeEnums.Subscription))
        {
            return receiverId.ToString();
        }

        if (IsObjectType(sender.ObjectType.Value, ChatObjectTypeEnums.Room, ChatObjectTypeEnums.Square, ChatObjectTypeEnums.Subscription))
        {
            return senderId.ToString();
        }

        await ResolveShopWaiterId(sender, receiver, async (v) =>
        {
            receiverId = v;
            await Task.Yield();
        });

        await ResolveShopWaiterId(receiver, sender, async (v) =>
        {
            senderId = v;
            await Task.Yield();
        });

        var arr = new[] { senderId, receiverId };

        Array.Sort(arr);

        return string.Join(":", arr);
    }

    public virtual Task<Session> MakeAsync(ChatObject room)
    {
        return MakeAsync(room, room);
    }

    public virtual async Task<Session> MakeAsync(ChatObject sender, ChatObject receiver)
    {
        var sessionKey = await MakeSesssionKeyAsync(sender, receiver);

        var sessionId = await MakeSesssionIdAsync(sessionKey);

        var session = await SessionRepository.FindAsync(x => x.Id.Equals(sessionId));

        if (session != null)
        {
            return session;
        }

        var channel = await ChannelResolver.GetAsync(sender, receiver);

        session = new Session(sessionId, sessionKey, channel);

        await ResolveSenderIsRobot(sender, async () =>
        {
            //add robot
            SessionUnitManager.Create(session: session,
                    owner: sender,
                    destination: receiver,
                    x =>
                    {
                        x.JoinWay = JoinWays.AutoJoin;
                    });

            //add receiver
            SessionUnitManager.Create(session: session,
                    owner: receiver,
                    destination: sender,
                    x =>
                    {
                        x.JoinWay = JoinWays.AutoJoin;
                    });

            await Task.Yield();
        });

        var customerTypes = new[] { ChatObjectTypeEnums.Personal, ChatObjectTypeEnums.Anonymous, ChatObjectTypeEnums.Customer };
        var shopTypes = new[] { ChatObjectTypeEnums.ShopKeeper, ChatObjectTypeEnums.ShopWaiter };
        var customer = sender.ObjectType.HasValue && customerTypes.Contains(sender.ObjectType.Value) ? sender :
            receiver.ObjectType.HasValue && customerTypes.Contains(receiver.ObjectType.Value) ? receiver : null;
        var shop = sender.ObjectType.HasValue && shopTypes.Contains(sender.ObjectType.Value) ? sender :
            receiver.ObjectType.HasValue && shopTypes.Contains(receiver.ObjectType.Value) ? receiver : null;
        if (customer != null && shop != null)
        {
            var shopKeeper = shop.ObjectType == ChatObjectTypeEnums.ShopKeeper ? shop :
                await ChatObjectManager.GetAsync(shop.ParentId ?? throw new InvalidOperationException("Shop waiter has no shopkeeper."));
            if (shopKeeper.ObjectType != ChatObjectTypeEnums.ShopKeeper)
            {
                throw new InvalidOperationException("Shop waiter's parent is not a shopkeeper.");
            }

            SessionUnitManager.Create(session: session,
                    owner: customer,
                    destination: shopKeeper,
                    x =>
                    {
                        x.JoinWay = JoinWays.AutoJoin;
                    });

            //add shopKeeper
            SessionUnitManager.Create(session: session,
                     owner: shopKeeper,
                     destination: customer,
                     x =>
                     {
                         x.JoinWay = JoinWays.AutoJoin;
                     });

            //Cache GetChildsByCacheAsync
            var shopWaiterList = await ChatObjectManager.GetChildsAsync(shopKeeper.Id);

            foreach (var shopWaiter in shopWaiterList.Where(x => x.ObjectType == ChatObjectTypeEnums.ShopWaiter))
            {
                SessionUnitManager.Create(session: session,
                    owner: shopWaiter,
                    destination: customer,
                    x =>
                    {
                        x.JoinWay = JoinWays.AutoJoin;
                    });
            }

        }


        //if (sender.ObjectType == ChatObjectTypeEnums.Official)
        //{
        //    Assert.If(receiver.ObjectType != ChatObjectTypeEnums.Personal && sender.Id != receiver.Id, "非法");
        //}

        //if (channel == Channels.PrivateChannel)
        //{
        //    session.AddSessionUnit(new SessionUnit(GuidGenerator.CreateAsync(), session, sender.Id, receiver.Id, receiver.ObjectType));

        //    if (sender.Id != receiver.Id)
        //    {
        //        session.AddSessionUnit(new SessionUnit(GuidGenerator.CreateAsync(), session, receiver.Id, sender.Id, sender.ObjectType));
        //    }
        //}
        return await SessionRepository.InsertAsync(session, autoSave: true);
    }

    public virtual async Task<List<SessionUnit>> AddShopWaitersIfNotContains(Session session, ChatObject destination, long shopKeeperId)
    {
        var shopWaiterList = await ChatObjectManager.GetChildsAsync(shopKeeperId);

        var list = new List<SessionUnit>();

        foreach (var shopWaiter in shopWaiterList)
        {
            var isAny = await SessionUnitManager.IsAnyAsync(shopWaiter.Id, destination.Id);

            if (!isAny)
            {
                list.Add(SessionUnitManager.Generate(
                    session: session,
                    owner: shopWaiter,
                    destination: destination,
                    x => x.JoinWay = JoinWays.AutoJoin));
            }
        }

        foreach (var item in list)
        {
            session.AddSessionUnit(item);
        }

        return list;
    }

    [UnitOfWork(true, IsolationLevel.ReadUncommitted)]
    public virtual async Task<List<Session>> GenerateSessionByMessageAsync()
    {
        var list = (await MessageRepository.GetQueryableAsync())
            .Where(x => x.SessionId == null)
            .GroupBy(x => x.SessionKey, (SessionValue, Items) => new
            {
                SessionValue,
                Items = Items.ToList()
            })
            .ToList();

        var sessionList = new List<Session>();

        foreach (var item in list)
        {
            var unitList = new List<SessionUnit>();

            var session = new Session(GuidGenerator.Create(), item.SessionValue, Channels.PrivateChannel);

            foreach (var message in item.Items)
            {
                if (!unitList.Any(x => x.OwnerId == message.SenderId.Value && x.DestinationId == message.ReceiverId.Value))
                {
                    unitList.Add(SessionUnitManager.Generate(session, message.Sender, message.Receiver, null));
                }
                if (!unitList.Any(x => x.OwnerId == message.ReceiverId.Value && x.DestinationId == message.SenderId.Value))
                {
                    unitList.Add(SessionUnitManager.Generate(session, message.Receiver, message.Sender, null));
                }
            }
            session.SetMessageList(item.Items);

            session.SetUnitList(unitList);

            sessionList.Add(session);
        }
        await SessionRepository.InsertManyAsync(sessionList);

        return sessionList;
    }
}
