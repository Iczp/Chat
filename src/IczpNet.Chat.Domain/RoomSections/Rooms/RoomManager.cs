using AutoMapper;
using AutoMapper.Execution;
using IczpNet.AbpCommons;
using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IczpNet.Chat.RoomSections.Rooms;

public class RoomManager : ChatObjectManager, IRoomManager
{
    protected virtual string GroupAssistantCode => "GroupAssistant";
    protected RoomOptions Config { get; }

    public RoomManager(IChatObjectRepository chatObjectRepository, IOptions<RoomOptions> options) : base(chatObjectRepository)
    {
        Config = options.Value;
    }

    public async Task<ChatObject> GetGroupAssistantAsync()
    {
        return Assert.NotNull(await Repository.FindAsync(x => x.Code == GroupAssistantCode), $"Entity no such by [code]:{GroupAssistantCode}");
    }

    public virtual Task<bool> IsAllowJoinRoomAsync(ChatObjectTypeEnums? objectType)
    {
        return Task.FromResult(ChatConsts.AllowJoinRoomObjectTypes.Any(x => x.Equals(objectType)));
    }

    public virtual Task<bool> IsAllowCreateRoomAsync(ChatObjectTypeEnums? objectType)
    {
        return Task.FromResult(Config.AllowCreateRoomObjectTypes.Any(x => x.Equals(objectType)));
    }

    public virtual async Task<ChatObject> CreateAsync(string name, List<long> memberIdList, long? ownerId)
    {
        var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.Room);

        var room = await base.CreateAsync(new ChatObject(name, chatObjectType, null), isUnique: false);

        var roomInfo = await base.MapToOuputAsync(room);

        var session = await SessionGenerator.MakeAsync(roomInfo);

        session.SetOwner(room);

        //add room sessionUnit
        //session.AddSessionUnit(new SessionUnit(
        //    id: GuidGenerator.Create(),
        //    session: session,
        //    ownerId: room.Id,
        //    destinationId: room.Id,
        //    destinationObjectType: room.ObjectType,
        //    isPublic: false,
        //    isStatic: true));

        //add Group Assistant
        var groupAssistant = await GetGroupAssistantAsync();

        var assistantSessionUnit = session.AddSessionUnit(new SessionUnit(
            id: GuidGenerator.Create(),
            session: session,
            ownerId: groupAssistant.Id,
            destinationId: room.Id,
            destinationObjectType: room.ObjectType,
            isPublic: false,
            isStatic: true));

        // add member
        foreach (var memberId in memberIdList)
        {
            session.AddSessionUnit(new SessionUnit(
                id: GuidGenerator.Create(),
                session: session,
                ownerId: memberId,
                destinationId: room.Id,
                destinationObjectType: room.ObjectType,
                isPublic: true,
                isStatic: memberId == ownerId));
        }

        room.OwnerSessionList.Add(session);

        // commit to db
        await CurrentUnitOfWork.SaveChangesAsync();

        var roomOwner = ownerId.HasValue ? await GetItemByCacheAsync(ownerId.Value) : null;

        var members = await GetManyByCacheAsync(memberIdList.Take(3).ToList());

        await SendRoomMessageAsync(assistantSessionUnit, new CmdContentInfo()
        {
            Text = $"{roomOwner?.Name}创建群聊'{room.Name}',{members.Select(x => x.Name).JoinAsString("、")}等 {memberIdList.Count} 人加入群聊。",
        });

        //await MessageSender.SendCmdMessageAsync(new MessageInput<CmdContentInfo>()
        //{
        //    SenderId = groupAssistant.Id,
        //    ReceiverId = room.Id,
        //    Content = new CmdContentInfo()
        //    {
        //        Text = $"{roomOwner?.Name}创建群聊'{room.Name}',{members.Select(x => x.Name).JoinAsString("、")}等 {memberIdList.Count} 人加入群聊。",
        //    }
        //});
        return room;
    }

    protected virtual Task SendRoomMessageAsync(SessionUnit roomSessionUnit, CmdContentInfo content)
    {
        return MessageSender.SendCmdAsync(new MessageSendInput<CmdContentInfo>()
        {
            SessionUnitId = roomSessionUnit.Id,
            Content = content
        });
    }



    public virtual async Task<ChatObject> CreateByAllUsersAsync(string name)
    {
        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.ObjectType == ChatObjectTypeEnums.Personal)
            .Select(x => x.Id)
            ;
        var idList = await AsyncExecuter.ToListAsync(query);

        return await CreateAsync(name, idList, null);
    }

    public Task<int> GetMemberCountAsync(ChatObject room)
    {
        throw new System.NotImplementedException();
    }

    public Task<int> JoinRoomAsync(ChatObject room, List<ChatObject> members, ChatObject inviter, JoinWays joinWay)
    {
        throw new System.NotImplementedException();
    }

    public Task<bool> IsInRoomAsync(ChatObject room, ChatObject member)
    {
        throw new System.NotImplementedException();
    }
}
