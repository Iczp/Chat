using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace IczpNet.Chat.RoomSections.Rooms;

public class RoomManager : DomainService, IRoomManager// ChatObjectManager, IRoomManager
{
    protected RoomOptions Config { get; }
    protected ISessionManager SessionManager { get; }
    protected ISessionUnitManager SessionUnitManager { get; }
    protected ISessionUnitRepository SessionUnitRepository { get; }
    protected IChatObjectManager ChatObjectManager { get; }
    protected IChatObjectTypeManager ChatObjectTypeManager { get; }
    protected ISessionGenerator SessionGenerator { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected IChatObjectRepository ChatObjectRepository { get; }
    protected IMessageSender MessageSender { get; }

    public RoomManager(
        IChatObjectRepository chatObjectRepository,
        IOptions<RoomOptions> options, ISessionManager sessionManager,
        ISessionUnitRepository sessionUnitRepository,
        ISessionUnitManager sessionUnitManager,
        IChatObjectManager chatObjectManager,
        IChatObjectTypeManager chatObjectTypeManager,
        ISessionGenerator sessionGenerator,
        IUnitOfWorkManager unitOfWorkManager,
        IMessageSender messageSender)
    {
        Config = options.Value;
        SessionManager = sessionManager;
        SessionUnitRepository = sessionUnitRepository;
        SessionUnitManager = sessionUnitManager;
        ChatObjectManager = chatObjectManager;
        ChatObjectTypeManager = chatObjectTypeManager;
        SessionGenerator = sessionGenerator;
        UnitOfWorkManager = unitOfWorkManager;
        ChatObjectRepository = chatObjectRepository;
        MessageSender = messageSender;
    }

    public virtual Task<bool> IsAllowJoinRoomAsync(ChatObjectTypeEnums objectType)
    {
        return Task.FromResult(IsAllowJoinRoom(objectType));
    }

    public virtual bool IsAllowJoinRoom(ChatObjectTypeEnums objectType)
    {
        return ChatConsts.AllowJoinRoomObjectTypes.Any(x => x.Equals(objectType));
    }

    public virtual Task<bool> IsAllowCreateRoomAsync(ChatObjectTypeEnums objectType)
    {
        return Task.FromResult(Config.AllowCreateRoomObjectTypes.Any(x => x.Equals(objectType)));
    }

    private SessionUnit AddRoomSessionUnit(Session session, long roomId)
    {
        return session.AddSessionUnit(new SessionUnit(
              id: GuidGenerator.Create(),
              session: session,
              ownerId: roomId,
              destinationId: roomId,
              destinationObjectType: ChatObjectTypeEnums.Room,
              isPublic: false,
              isStatic: true,
              isCreator: false,
              joinWay: JoinWays.System,
              inviterUnitId: null));
    }

    public virtual async Task<ChatObject> CreateAsync(string name, List<long> memberIdList, long? ownerId)
    {
        var allList = memberIdList;

        if (ownerId != null && !allList.Contains(ownerId.Value))
        {
            allList.Add(ownerId.Value);
        }

        var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.Room);

        var room = await ChatObjectManager.CreateAsync(new ChatObject(name, chatObjectType, null), isUnique: false);

        var session = await SessionGenerator.MakeAsync(room);

        session.SetOwner(room);

        //add room sessionUnit
        var roomSessionUnit = AddRoomSessionUnit(session, room.Id);

        ////add Group Assistant
        //var groupAssistant = await GetGroupAssistantAsync();
        //var assistantSessionUnit = session.AddSessionUnit(new SessionUnit(
        //    id: GuidGenerator.Create(),
        //    session: session,
        //    ownerId: groupAssistant.Id,
        //    destinationId: room.Id,
        //    destinationObjectType: room.ObjectType,
        //    isPublic: false,
        //    isStatic: true));
        SessionUnit inviterSessionUnit = null;
        //group owner
        if (ownerId != null)
        {
            inviterSessionUnit = session.AddSessionUnit(new SessionUnit(
                id: GuidGenerator.Create(),
                session: session,
                ownerId: ownerId.Value,
                destinationId: room.Id,
                destinationObjectType: room.ObjectType,
                isPublic: true,
                isStatic: true,
                isCreator: true,
                joinWay: JoinWays.Creator,
                inviterUnitId: null,
                isInputEnabled: true));
        }
        // add member
        var _memberIdList = allList
            .Where(x => x != ownerId).ToList();

        _ = _memberIdList.Select(memberId => session.AddSessionUnit(new SessionUnit(
                id: GuidGenerator.Create(),
                session: session,
                ownerId: memberId,
                destinationId: room.Id,
                destinationObjectType: room.ObjectType,
                isPublic: true,
                isStatic: false,
                isCreator: false,
                joinWay: JoinWays.Invitation,
                inviterUnitId: inviterSessionUnit?.Id,
                isInputEnabled: true)))
            .ToList();

        room.OwnerSessionList.Add(session);

        // commit to db
        await UnitOfWorkManager.Current.SaveChangesAsync();

        var roomOwner = ownerId.HasValue ? await ChatObjectManager.GetItemByCacheAsync(ownerId.Value) : null;

        var members = await ChatObjectManager.GetManyByCacheAsync(_memberIdList.Take(3).ToList());

        await SendRoomMessageAsync(roomSessionUnit, new CmdContentInfo()
        {
            Text = $"{roomOwner?.Name} 创建群聊'{room.Name}',{members.Select(x => x.Name).JoinAsString("、")}等 {_memberIdList.Count} 人加入群聊。",
        });

        return room;
    }

    public virtual async Task<ChatObject> CreateWithManyAsync(string name, List<long> memberIdList, long? ownerId)
    {
        var allList = memberIdList;

        if (ownerId != null && !allList.Contains(ownerId.Value))
        {
            allList.Add(ownerId.Value);
        }

        var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.Room);

        var room = await ChatObjectManager.CreateAsync(new ChatObject(name, chatObjectType, null), isUnique: false);

        var session = await SessionGenerator.MakeAsync(room);

        session.SetOwner(room);

        //add room sessionUnit
        var roomSessionUnit = new SessionUnit(
              id: GuidGenerator.Create(),
              session: session,
              ownerId: room.Id,
              destinationId: room.Id,
              destinationObjectType: ChatObjectTypeEnums.Room,
              isPublic: false,
              isStatic: true,
              isCreator: false,
              joinWay: JoinWays.System,
              inviterUnitId: null);

        SessionUnit inviterSessionUnit = null;
        //group owner
        if (ownerId != null)
        {
            inviterSessionUnit = new SessionUnit(
                id: GuidGenerator.Create(),
                session: session,
                ownerId: ownerId.Value,
                destinationId: room.Id,
                destinationObjectType: room.ObjectType,
                isPublic: true,
                isStatic: true,
                isCreator: true,
                joinWay: JoinWays.Creator,
                inviterUnitId: null,
                isInputEnabled: true);
        }
        // add member
        var _memberIdList = allList
            .Where(x => x != ownerId).ToList();

        var memberSessionUnitList = _memberIdList.Select(memberId => session.AddSessionUnit(new SessionUnit(
                id: GuidGenerator.Create(),
                session: session,
                ownerId: memberId,
                destinationId: room.Id,
                destinationObjectType: room.ObjectType,
                isPublic: true,
                isStatic: false,
                isCreator: false,
                joinWay: JoinWays.Invitation,
                inviterUnitId: inviterSessionUnit?.Id,
                isInputEnabled: true)))
            .ToList();

        room.OwnerSessionList.Add(session);

        var insertSessionUnitList = new List<SessionUnit>() { roomSessionUnit }.Concat(memberSessionUnitList).ToList();

        await SessionUnitRepository.InsertManyAsync(insertSessionUnitList);
        // commit to db
        await UnitOfWorkManager.Current.SaveChangesAsync();

        var roomOwner = ownerId.HasValue ? await ChatObjectManager.GetItemByCacheAsync(ownerId.Value) : null;

        var members = await ChatObjectManager.GetManyByCacheAsync(_memberIdList.Take(3).ToList());

        await SendRoomMessageAsync(roomSessionUnit, new CmdContentInfo()
        {
            Text = $"{roomOwner?.Name} 创建群聊'{room.Name}',{members.Select(x => x.Name).JoinAsString("、")}等 {_memberIdList.Count} 人加入群聊。",
        });

        return room;
    }

    public virtual async Task<List<SessionUnit>> InviteAsync(InviteInput input, bool autoSendMessage = true)
    {
        //var room = await GetAsync(roomId);

        var session = await SessionManager.GetByOwnerIdAsync(input.RoomId);

        SessionUnit inviterSessionUnit = null;

        if (input.InviterId.HasValue)
        {
            inviterSessionUnit = await SessionUnitManager.FindBySessionIdAsync(session.Id, input.InviterId.Value);
            Assert.If(inviterSessionUnit == null, "邀请人不在群里");
        }

        //Assert.If(input.InviterId.HasValue && !await IsInRoomAsync(session.Id, input.InviterId.Value), "邀请人不在群里");

        var inMemberIdList = (await SessionUnitRepository.GetQueryableAsync())
            .Where(x => x.SessionId == session.Id && input.MemberIdList.Contains(x.OwnerId))
            .Select(x => x.OwnerId)
            .ToList();

        Assert.If(inMemberIdList.Any(), $"有 {inMemberIdList.Count} 人已经在群里,[id:{inMemberIdList.FirstOrDefault()}]");

        var newMemberIdList = input.MemberIdList.Except(inMemberIdList).ToList();

        Assert.If(newMemberIdList.Count == 0, "没有数据:newMemberIdList");

        var joinMembers = await ChatObjectManager.GetManyByCacheAsync(newMemberIdList);

        Assert.If(joinMembers.Count == 0, "没有数据:joinMembers");

        var result = new List<SessionUnit>();

        foreach (var member in joinMembers)
        {
            Assert.If(!IsAllowJoinRoom(member.ObjectType.GetValueOrDefault()), $"不能加入群:[id:${member.Id},ObjectType:{member.ObjectType}]");

            result.Add(session.AddSessionUnit(new SessionUnit(
               id: GuidGenerator.Create(),
               session: session,
               ownerId: member.Id,
               destinationId: input.RoomId,
               destinationObjectType: ChatObjectTypeEnums.Room,
               isPublic: true,
               isStatic: false,
               isCreator: false,
               joinWay: JoinWays.Invitation,
               inviterUnitId: inviterSessionUnit?.Id,
               isInputEnabled: true)));
        }
        await UnitOfWorkManager.Current.SaveChangesAsync();

        if (!autoSendMessage)
        {
            return result;
        }

        var inviterText = string.Empty;

        if (input.InviterId != null)
        {
            var inviter = await ChatObjectManager.GetAsync(input.InviterId.Value);
            inviterText = $"{inviter.Name} 邀请 ";
        }

        await SendRoomMessageAsync(input.RoomId, new CmdContentInfo()
        {
            Text = $"{inviterText}{joinMembers.Select(x => x.Name).JoinAsString("、")}等 {joinMembers.Count} 人加入群聊。",
        });

        return result;
    }

    protected virtual Task SendRoomMessageAsync(SessionUnit roomSessionUnit, CmdContentInfo content)
    {
        return MessageSender.SendCmdAsync(roomSessionUnit, new MessageSendInput<CmdContentInfo>()
        {
            Content = content
        });
    }

    protected virtual async Task SendRoomMessageAsync(long roomId, CmdContentInfo content)
    {
        var roomSessionUnit = await SessionUnitManager.FindAsync(roomId, roomId);

        if (roomSessionUnit == null)
        {
            var session = await SessionManager.GetByOwnerIdAsync(roomId);

            //add room sessionUnit
            roomSessionUnit = AddRoomSessionUnit(session, roomId);

            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        await SendRoomMessageAsync(roomSessionUnit, content);
    }

    public virtual async Task<ChatObject> CreateByAllUsersAsync(string name)
    {
        var query = (await ChatObjectRepository.GetQueryableAsync())
            .Where(x => x.ObjectType == ChatObjectTypeEnums.Personal)
            .Select(x => x.Id)
            ;
        var idList = await AsyncExecuter.ToListAsync(query);

        return await CreateAsync(name, idList, null);
    }

    public virtual async Task<ChatObject> CreateByAllUsersWithManyAsync(string name)
    {
        var query = (await ChatObjectRepository.GetQueryableAsync())
            .Where(x => x.ObjectType == ChatObjectTypeEnums.Personal)
            .Select(x => x.Id)
            ;
        var idList = await AsyncExecuter.ToListAsync(query);

        return await CreateWithManyAsync(name, idList, null);
    }

    public virtual Task<int> GetMemberCountAsync(ChatObject room)
    {
        throw new System.NotImplementedException();
    }

    public virtual Task<int> JoinRoomAsync(ChatObject room, List<ChatObject> members, ChatObject inviter, JoinWays joinWay)
    {
        throw new System.NotImplementedException();
    }

    public virtual Task<bool> IsInRoomAsync(ChatObject room, ChatObject member)
    {
        throw new System.NotImplementedException();

    }

    public virtual async Task<bool> IsInRoomAsync(Guid sessionId, IEnumerable<long> memberIdList)
    {
        Assert.If(!memberIdList.Any(), "memberIdList count:0");

        var inMemberIdList = (await SessionUnitRepository.GetQueryableAsync())
            .Where(x => x.SessionId == sessionId && memberIdList.Contains(x.OwnerId))
            .Select(x => x.OwnerId)
            .ToList();

        if (!inMemberIdList.Any())
        {
            return false;
        }
        return !memberIdList.Except(inMemberIdList).Any();
    }

    public virtual Task<bool> IsInRoomAsync(Guid sessionId, long memberId)
    {
        return IsInRoomAsync(sessionId, new List<long>() { memberId });
    }

    private async Task<IQueryable<SessionUnit>> QuerySessionUnitByOwnerAsync(long ownerId, List<ChatObjectTypeEnums> chatObjectTypeList = null)
    {
        return (await SessionUnitRepository.GetQueryableAsync())
              .Where(x => x.OwnerId.Equals(ownerId) && !x.IsKilled && x.IsEnabled)
              .WhereIf(chatObjectTypeList.IsAny(), x => chatObjectTypeList.Contains(x.DestinationObjectType.Value));

    }

    public virtual async Task<IQueryable<SessionUnit>> GetSameGroupAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null)
    {
        var targetSessionIdList = (await QuerySessionUnitByOwnerAsync(targetChatObjectId, chatObjectTypeList))
            .Select(x => x.SessionId);

        var sourceQuery = (await QuerySessionUnitByOwnerAsync(sourceChatObjectId, chatObjectTypeList))
            .Where(x => targetSessionIdList.Contains(x.SessionId))
            ;

        return sourceQuery;
    }

    public async Task<ChatObject> UpdateNameAsync(SessionUnit sessionUnit, string name)
    {
        var entity = await ChatObjectManager.SetEnitiyAsync(sessionUnit.DestinationId.Value, x => x.SetName(name), isUnique: false);

        await SendRoomMessageAsync(sessionUnit.DestinationId.Value, new CmdContentInfo()
        {
            Text = $"<a session-unit-id=\"{sessionUnit.Id}\">{sessionUnit.Owner.Name}</a>更新群名称:'{name}'",
        });
        return entity;
    }

    public async Task<ChatObject> UpdatePortraitAsync(SessionUnit sessionUnit, string portrait)
    {
        var entity = await ChatObjectManager.SetEnitiyAsync(sessionUnit.DestinationId.Value, x => x.SetPortrait(portrait), isUnique: false);

        await SendRoomMessageAsync(sessionUnit.DestinationId.Value, new CmdContentInfo()
        {
            Text = $"<a session-unit-id=\"{sessionUnit.Id}\">{sessionUnit.Owner.Name}</a>更新群头像",
        });
        return entity;
    }
}
