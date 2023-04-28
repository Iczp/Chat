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
using IczpNet.Chat.TextTemplates;
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
    protected ISessionUnitIdGenerator SessionUnitIdGenerator { get; }

    public RoomManager(
        IChatObjectRepository chatObjectRepository,
        IOptions<RoomOptions> options, ISessionManager sessionManager,
        ISessionUnitRepository sessionUnitRepository,
        ISessionUnitManager sessionUnitManager,
        IChatObjectManager chatObjectManager,
        IChatObjectTypeManager chatObjectTypeManager,
        ISessionGenerator sessionGenerator,
        IUnitOfWorkManager unitOfWorkManager,
        IMessageSender messageSender,
        ISessionUnitIdGenerator sessionUnitIdGenerator)
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
        SessionUnitIdGenerator = sessionUnitIdGenerator;
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
              idGenerator: SessionUnitIdGenerator,
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
        var roomSessionUnit = new SessionUnit(
              idGenerator: SessionUnitIdGenerator,
              session: session,
              ownerId: room.Id,
              destinationId: room.Id,
              destinationObjectType: ChatObjectTypeEnums.Room,
              isPublic: false,
              isStatic: true,
              isCreator: false,
              joinWay: JoinWays.System,
              inviterUnitId: null);

        SessionUnit creatorSessionUnit = null;
        //group owner
        if (ownerId != null)
        {
            creatorSessionUnit = new SessionUnit(
                idGenerator: SessionUnitIdGenerator,
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

        var memberSessionUnitList = _memberIdList.Select(memberId => new SessionUnit(
                idGenerator: SessionUnitIdGenerator,
                session: session,
                ownerId: memberId,
                destinationId: room.Id,
                destinationObjectType: room.ObjectType,
                isPublic: true,
                isStatic: false,
                isCreator: false,
                joinWay: JoinWays.Invitation,
                inviterUnitId: creatorSessionUnit?.Id,
                isInputEnabled: true))
            .ToList();

        room.OwnerSessionList.Add(session);

        var insertSessionUnitList = new List<SessionUnit>() { roomSessionUnit, creatorSessionUnit }.Concat(memberSessionUnitList).ToList();

        await SessionUnitRepository.InsertManyAsync(insertSessionUnitList);

        // commit to db
        await UnitOfWorkManager.Current.SaveChangesAsync();


        var members = await ChatObjectManager.GetManyByCacheAsync(_memberIdList.Take(3).ToList());

        var membersJoinText = members
                            .Select(x => new SessionUnitTextTemplate(insertSessionUnitList.FirstOrDefault(d => d.OwnerId == x.Id).Id, x.Name).ToString())
                            .JoinAsString("、");

        var creatorText = string.Empty;

        if (creatorSessionUnit != null)
        {
            var creator = await ChatObjectManager.GetItemByCacheAsync(ownerId.Value);

            creatorText = new TextTemplate("{Creator}创建群聊,")
                .WithData("Creator", new SessionUnitTextTemplate(creatorSessionUnit.Id, creator.Name))
                .ToString();
        }

        await SendRoomMessageAsync(roomSessionUnit, new CmdContentInfo()
        {
            //Text = $"{roomOwner?.Name} 创建群聊'{room.Name}',{vtext}等 {_memberIdList.Count} 人加入群聊。",
            Text = new TextTemplate("{CreatorText}{MembersJoinText}等 {Count} 人加入群聊")
                        .WithData("CreatorText", creatorText)
                        .WithData("MembersJoinText", membersJoinText)
                        .WithData("Count", _memberIdList.Count)
                        .ToString(),
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

        var inMemberIdList = (await SessionUnitRepository.GetQueryableAsync())
            .Where(x => x.SessionId == session.Id && input.MemberIdList.Contains(x.OwnerId))
            .Select(x => x.OwnerId)
            .ToList();

        Assert.If(inMemberIdList.Any(), $"有 {inMemberIdList.Count} 人已经在群里,[id:{inMemberIdList.FirstOrDefault()}]");

        var newMemberIdList = input.MemberIdList.Except(inMemberIdList).ToList();

        Assert.If(newMemberIdList.Count == 0, "没有数据:newMemberIdList");

        var joinMembers = await ChatObjectManager.GetManyByCacheAsync(newMemberIdList);

        Assert.If(joinMembers.Count == 0, "没有数据:joinMembers");

        foreach (var member in joinMembers)
        {
            Assert.If(!IsAllowJoinRoom(member.ObjectType.GetValueOrDefault()), $"不能加入群:[id:${member.Id},ObjectType:{member.ObjectType}]");
        }

        var joinMemberSessionUnitList = joinMembers.Select(x =>
            new SessionUnit(
                idGenerator: SessionUnitIdGenerator,
                session: session,
                ownerId: x.Id,
                destinationId: input.RoomId,
                destinationObjectType: ChatObjectTypeEnums.Room,
                isPublic: true,
                isStatic: false,
                isCreator: false,
                joinWay: JoinWays.Invitation,
                inviterUnitId: inviterSessionUnit?.Id,
                isInputEnabled: true))
            .ToList();

        await SessionUnitRepository.InsertManyAsync(joinMemberSessionUnitList);

        //await UnitOfWorkManager.Current.SaveChangesAsync();

        if (!autoSendMessage)
        {
            return joinMemberSessionUnitList;
        }

        var inviterText = string.Empty;

        if (input.InviterId != null)
        {
            var inviter = await ChatObjectManager.GetAsync(input.InviterId.Value);
            inviterText = new TextTemplate("{InviterObject} 邀请 ")
                            .WithData("InviterObject", new SessionUnitTextTemplate(inviterSessionUnit))
                            .ToString();
        }

        var membersJoinText = joinMemberSessionUnitList.Take(3)
                            .Select(x => new SessionUnitTextTemplate(x.Id, joinMembers.FirstOrDefault(d => d.Id == x.OwnerId)?.Name).ToString())
                            .JoinAsString("、");

        await SendRoomMessageAsync(input.RoomId, new CmdContentInfo()
        {
            Cmd = MessageKeyNames.JoinRoom,
            //Text = $"{inviterText}{joinMembers.Select(x => x.Name).JoinAsString("、")}等 {joinMembers.Count} 人加入群聊。",
            Text = new TextTemplate("{InviterText}{MembersJoinText}等 {Count} 人加入群聊")
                        .WithData("InviterText", inviterText)
                        .WithData("MembersJoinText", membersJoinText)
                        .WithData("Count", joinMembers.Count)
                        .ToString(),
        });

        return joinMemberSessionUnitList;
    }

    protected virtual Task SendRoomMessageAsync(SessionUnit roomSessionUnit, CmdContentInfo content)
    {
        return MessageSender.SendCmdAsync(roomSessionUnit, new MessageSendInput<CmdContentInfo>()
        {
            Content = content
        });
    }

    //发送群消息
    protected virtual async Task SendRoomMessageAsync(long roomId, CmdContentInfo content)
    {
        //Find the room session unit
        var roomSessionUnit = await SessionUnitManager.FindAsync(roomId, roomId);

        //If the room session unit is not found
        if (roomSessionUnit == null)
        {
            //Get the session by owner id
            var session = await SessionManager.GetByOwnerIdAsync(roomId);

            //Add the room session unit
            roomSessionUnit = AddRoomSessionUnit(session, roomId);

            //Save the changes
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        //Send the room message
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
        var entity = await ChatObjectManager.UpdateAsync(sessionUnit.DestinationId.Value, x => x.SetName(name), isUnique: false);

        await SendRoomMessageAsync(sessionUnit.DestinationId.Value, new CmdContentInfo()
        {
            //Cmd = Message
            Text = new TextTemplate("{Operator} 更新群名称:'{RoomName}'")
                    .WithData("Operator", new SessionUnitTextTemplate(sessionUnit))
                    .WithData("RoomName", name)
                    .ToString(),
        });
        return entity;
    }

    public async Task<ChatObject> UpdatePortraitAsync(SessionUnit sessionUnit, string portrait)
    {
        var entity = await ChatObjectManager.UpdateAsync(sessionUnit.DestinationId.Value, x => x.SetPortrait(portrait), isUnique: false);

        await SendRoomMessageAsync(sessionUnit.DestinationId.Value, new CmdContentInfo()
        {
            Text = new TextTemplate("{Operator} 更新群头像")
                    .WithData("Operator", new SessionUnitTextTemplate(sessionUnit))
                    .ToString(),
        });
        return entity;
    }

    public async Task TransferCreatorAsync(Guid sessionUnitId, Guid targetSessionUnitId, bool isSendMessageToRoom = true)
    {
        var sessionUnit = await SessionUnitRepository.GetAsync(sessionUnitId);

        Assert.If(!sessionUnit.IsCreator, "Not the session creator");

        var targetSessionUnit = await SessionUnitRepository.GetAsync(targetSessionUnitId);

        Assert.If(sessionUnit.SessionId != targetSessionUnit.SessionId, "Not in same session");

        sessionUnit.SetIsCreator(false);

        targetSessionUnit.SetIsCreator(true);

        if (isSendMessageToRoom)
        {
            await SendRoomMessageAsync(sessionUnit.DestinationId.Value, new CmdContentInfo()
            {
                Text = new TextTemplate("{Operator} 转让群,{TargetObject} 成为群主")
                    .WithData("Operator", new SessionUnitTextTemplate(sessionUnit))
                    .WithData("TargetObject", new SessionUnitTextTemplate(targetSessionUnit))
                    .ToString(),
            });
        }
    }
}
