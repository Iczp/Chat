using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;

namespace IczpNet.Chat.RoomSections.Rooms;

public class RoomManager : ChatObjectManager, IRoomManager
{
    protected virtual string GroupAssistantCode => "GroupAssistant";
    protected RoomOptions Config { get; }
    protected ISessionManager SessionManager { get; }
    protected ISessionUnitManager SessionUnitManager { get; }
    protected ISessionUnitRepository SessionUnitRepository { get; }

    public RoomManager(
        IChatObjectRepository chatObjectRepository,
        IOptions<RoomOptions> options, ISessionManager sessionManager,
        ISessionUnitRepository sessionUnitRepository,
        ISessionUnitManager sessionUnitManager) : base(chatObjectRepository)
    {
        Config = options.Value;
        SessionManager = sessionManager;
        SessionUnitRepository = sessionUnitRepository;
        SessionUnitManager = sessionUnitManager;
    }

    public async Task<ChatObject> GetGroupAssistantAsync()
    {
        return Assert.NotNull(await Repository.FindAsync(x => x.Code == GroupAssistantCode), $"Entity no such by [code]:{GroupAssistantCode}");
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
              joinWay: JoinWays.System));
    }
    public virtual async Task<ChatObject> CreateAsync(string name, List<long> memberIdList, long? ownerId)
    {
        var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.Room);

        var room = await base.CreateAsync(new ChatObject(name, chatObjectType, null), isUnique: false);

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

        await SendRoomMessageAsync(roomSessionUnit, new CmdContentInfo()
        {
            Text = $"{roomOwner?.Name}创建群聊'{room.Name}',{members.Select(x => x.Name).JoinAsString("、")}等 {memberIdList.Count} 人加入群聊。",
        });

        return room;
    }

    public virtual async Task<List<SessionUnit>> InviteAsync(InviteInput input, bool autoSendMessage = true)
    {
        //var room = await GetAsync(roomId);

        var session = await SessionManager.GetByOwnerIdAsync(input.RoomId);

        Assert.If(input.InviterId.HasValue && !await IsInRoomAsync(session.Id, input.InviterId.Value), "邀请人不在群里");

        var inMemberIdList = (await SessionUnitRepository.GetQueryableAsync())
            .Where(x => x.SessionId == session.Id && input.MemberIdList.Contains(x.OwnerId))
            .Select(x => x.OwnerId)
            .ToList();

        Assert.If(inMemberIdList.Any(), $"有 {inMemberIdList.Count} 人已经在群里,[id:{inMemberIdList.FirstOrDefault()}]");

        var newMemberIdList = input.MemberIdList.Except(inMemberIdList).ToList();

        Assert.If(newMemberIdList.Count == 0, "没有数据:newMemberIdList");

        var joinMembers = await GetManyByCacheAsync(newMemberIdList);

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
               joinWay: JoinWays.Invitation)));
        }
        await CurrentUnitOfWork.SaveChangesAsync();

        if (!autoSendMessage)
        {
            return result;
        }

        var inviterText = string.Empty;

        if (input.InviterId != null)
        {
            var inviter = await GetAsync(input.InviterId.Value);
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
        return MessageSender.SendCmdAsync(new MessageSendInput<CmdContentInfo>()
        {
            SessionUnitId = roomSessionUnit.Id,
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

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        await SendRoomMessageAsync(roomSessionUnit, content);
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

    public async Task<bool> IsInRoomAsync(Guid sessionId, IEnumerable<long> memberIdList)
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

    public Task<bool> IsInRoomAsync(Guid sessionId, long memberId)
    {
        return IsInRoomAsync(sessionId, new List<long>() { memberId });
    }
}
