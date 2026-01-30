using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Options;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
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

public class RoomManager(
    IOnlineManager onlineManager,
    IChatObjectRepository chatObjectRepository,
    IOptions<RoomOptions> options,
    ISessionManager sessionManager,
    ISessionUnitCacheManager sessionUnitCacheManager,
    ISessionUnitRepository sessionUnitRepository,
    ISessionUnitManager sessionUnitManager,
    IChatObjectManager chatObjectManager,
    IChatObjectTypeManager chatObjectTypeManager,
    ISessionGenerator sessionGenerator,
    IUnitOfWorkManager unitOfWorkManager,
    IMessageSender messageSender,
    IRoomCodeGenerator roomCodeGenerator,
    ISessionUnitIdGenerator sessionUnitIdGenerator) : DomainService, IRoomManager// ChatObjectManager, IRoomManager
{
    protected RoomOptions Config { get; } = options.Value;
    protected ISessionManager SessionManager { get; } = sessionManager;
    public ISessionUnitCacheManager SessionUnitCacheManager { get; } = sessionUnitCacheManager;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    protected ISessionUnitRepository SessionUnitRepository { get; } = sessionUnitRepository;
    protected IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    protected IChatObjectTypeManager ChatObjectTypeManager { get; } = chatObjectTypeManager;
    protected ISessionGenerator SessionGenerator { get; } = sessionGenerator;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    public IOnlineManager OnlineManager { get; } = onlineManager;
    protected IChatObjectRepository ChatObjectRepository { get; } = chatObjectRepository;
    protected IMessageSender MessageSender { get; } = messageSender;
    protected IRoomCodeGenerator RoomCodeGenerator { get; } = roomCodeGenerator;
    protected ISessionUnitIdGenerator SessionUnitIdGenerator { get; } = sessionUnitIdGenerator;

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

    private SessionUnit AddRoomSessionUnit(Session session, ChatObject room)
    {
        return SessionUnitManager.Create(
              session: session,
              owner: room,
              destination: room,
              x =>
              {
                  x.IsPublic = false;
                  x.IsStatic = true;
                  x.JoinWay = JoinWays.System;
              });
    }

    public virtual async Task<ChatObject> CreateAsync(string name, List<long> memberIdList, long? ownerId)
    {
        var allList = memberIdList;

        if (ownerId != null && !allList.Contains(ownerId.Value))
        {
            allList.Add(ownerId.Value);
        }

        var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.Room);

        var code = await RoomCodeGenerator.MakeAsync();

        var room = await ChatObjectManager.CreateAsync(new ChatObject(name, code, chatObjectType, null), isUnique: false);

        var session = await SessionGenerator.MakeAsync(room);

        session.SetOwner(room);

        //add room sessionUnit
        var roomSessionUnit = AddRoomSessionUnit(session, room);

        SessionUnit creatorSessionUnit = null;
        //group owner
        if (ownerId != null)
        {
            var owner = await ChatObjectManager.GetAsync(ownerId.Value);

            creatorSessionUnit = SessionUnitManager.Generate(
                session: session,
                owner: owner,
                destination: room,
                x =>
                {
                    x.IsPublic = true;
                    x.IsStatic = true;
                    x.SetIsCreator(true);
                    x.JoinWay = JoinWays.Creator;
                });
        }
        // add member
        var _memberIdList = allList
            .Where(x => x != ownerId).ToList();

        var memberSessionUnitList = new List<SessionUnit>();

        foreach (var memberId in _memberIdList)
        {
            memberSessionUnitList.Add(SessionUnitManager.Generate(
                session: session,
                owner: await ChatObjectManager.GetAsync(memberId),
                destination: room,
                x =>
                {
                    x.IsStatic = false;
                    x.JoinWay = JoinWays.Invitation;
                    x.InviterId = creatorSessionUnit?.Id;
                }));
        }

        room.OwnerSessionList.Add(session);

        var insertSessionUnitList = new List<SessionUnit>() { roomSessionUnit, creatorSessionUnit }.Concat(memberSessionUnitList).Where(x => x != null).ToList();

        await SessionUnitRepository.InsertManyAsync(insertSessionUnitList, autoSave: true);

        //添加会话到连接池
        await AddUnitsToConnectionPoolsAsync(insertSessionUnitList);

        // commit to db
        //await UnitOfWorkManager.Current.SaveChangesAsync();

        var members = await ChatObjectManager.GetManyByCacheAsync(_memberIdList.Take(3).ToList());

        var membersJoinText = members
                            .Select(x => new SessionUnitTextTemplate(insertSessionUnitList.FirstOrDefault(d => d.OwnerId == x.Id).Id, x.Name).ToString())
                            .JoinAsString("、");

        var creatorText = string.Empty;

        if (creatorSessionUnit != null)
        {
            var creator = await ChatObjectManager.GetItemByCacheAsync(ownerId.Value);

            creatorText = new TextTemplate("{creator} 创建群聊 '{name}',")
                .WithData("name", new SessionUnitTextTemplate(roomSessionUnit.Id, room.Name))
                .WithData("creator", new SessionUnitTextTemplate(creatorSessionUnit.Id, creator.Name))
                .ToString();
        }

        await SendRoomMessageAsync(roomSessionUnit, new CmdContentInfo()
        {
            //Text = $"{roomOwner?.Name} 创建群聊'{room.Name}',{vtext}等 {_memberIdList.Count} 人加入群聊。",
            Cmd = MessageKeyNames.JoinRoom,
            Text = new TextTemplate("{creatorText} {membersJoinText}等 {count} 人加入群聊")
                        .WithData("creatorText", creatorText)
                        .WithData("membersJoinText", membersJoinText)
                        .WithData("count", _memberIdList.Count)
                        .ToString(),
        });

        return room;
    }

    public virtual async Task<List<SessionUnit>> InviteAsync(InviteInput input, bool autoSendMessage = true)
    {
        //var room = await GetWalletAsync(roomId);

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

        var joinMembers = await ChatObjectManager.GetManyAsync(newMemberIdList);

        Assert.If(joinMembers.Count == 0, "没有数据:joinMembers");

        foreach (var member in joinMembers)
        {
            Assert.If(!IsAllowJoinRoom(member.ObjectType.GetValueOrDefault()), $"不能加入群:[id:${member.Id},ObjectType:{member.ObjectType}]");
        }

        var room = await ChatObjectManager.GetAsync(input.RoomId);

        var joinMemberSessionUnitList = joinMembers.Select(x =>
            SessionUnitManager.Generate(
                session: session,
                owner: x,
                destination: room,
                x =>
                {
                    x.JoinWay = JoinWays.Invitation;
                    x.InviterId = inviterSessionUnit?.Id;
                }))
            .ToList();

        await SessionUnitRepository.InsertManyAsync(joinMemberSessionUnitList, autoSave: true);

        //添加会话到缓存
        await SessionUnitManager.AddUnitsToCacheAsync(joinMemberSessionUnitList);

        //添加会话到连接池
        await AddUnitsToConnectionPoolsAsync(joinMemberSessionUnitList);

        //await UnitOfWorkManager.Current.SaveChangesAsync();

        if (!autoSendMessage)
        {
            return joinMemberSessionUnitList;
        }

        var inviterText = string.Empty;

        if (input.InviterId != null)
        {
            var inviter = await ChatObjectManager.GetAsync(input.InviterId.Value);

            inviterText = new TextTemplate("{inviterObject} 邀请 ")
                            .WithData("inviterObject", new SessionUnitTextTemplate(inviterSessionUnit))
                            .ToString();
        }

        var membersJoinText = joinMemberSessionUnitList.Take(3)
                            .Select(x => new SessionUnitTextTemplate(x.Id, joinMembers.FirstOrDefault(d => d.Id == x.OwnerId)?.Name).ToString())
                            .JoinAsString("、");

        await SendRoomMessageAsync(room, new CmdContentInfo()
        {
            Cmd = MessageKeyNames.JoinRoom,
            //Text = $"{inviterText}{joinMembers.Select(x => x.Name).JoinAsString("、")}等 {joinMembers.Count} 人加入群聊。",
            Text = new TextTemplate("{inviterText} {membersJoinText} 等 {count} 人加入群聊")
                        .WithData("inviterText", inviterText)
                        .WithData("membersJoinText", membersJoinText)
                        .WithData("count", joinMembers.Count)
                        .ToString(),
        });

        return joinMemberSessionUnitList;
    }

    /// <summary>
    /// 添加会话到连接池
    /// </summary>
    /// <param name="joinMemberSessionUnitList"></param>
    /// <returns></returns>
    private async Task AddUnitsToConnectionPoolsAsync(List<SessionUnit> joinMemberSessionUnitList)
    {
        var units = joinMemberSessionUnitList.Select(x => (SessionId: x.SessionId.Value, x.OwnerId)).ToList();
        await OnlineManager.AddSessionAsync(units);
    }

    protected virtual Task SendRoomMessageAsync(SessionUnit roomSessionUnit, CmdContentInfo content)
    {
        Assert.If(roomSessionUnit.OwnerObjectType != ChatObjectTypeEnums.Room, $"Fail ObjectType:{roomSessionUnit.OwnerObjectType}");

        return MessageSender.SendCmdAsync(roomSessionUnit, new MessageInput<CmdContentInfo>()
        {
            Content = content
        });
    }

    //发送群消息
    protected virtual Task SendRoomMessageAsync(ChatObject room, CmdContentInfo content)
    {
        return SendRoomMessageAsync(room, x => content);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="room"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    protected virtual async Task SendRoomMessageAsync(ChatObject room, Func<SessionUnit, CmdContentInfo> func)
    {
        Assert.If(room.ObjectType != ChatObjectTypeEnums.Room, $"Fail ObjectType:{room.ObjectType}");

        //Find the room session unit
        var roomSessionUnit = await SessionUnitManager.FindAsync(room.Id, room.Id);

        //If the room session unit is not found
        if (roomSessionUnit == null)
        {
            //Get the session by owner id
            var session = await SessionManager.GetByOwnerIdAsync(room.Id);

            //Add the room session unit
            roomSessionUnit = AddRoomSessionUnit(session, room);

            //Save the changes
            await UnitOfWorkManager.Current.SaveChangesAsync();
        }

        var content = func(roomSessionUnit);

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
              .Where(x => x.OwnerId.Equals(ownerId) && !x.Setting.IsKilled && x.Setting.IsEnabled)
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
        var entity = await ChatObjectManager.UpdateNameAsync(sessionUnit.DestinationId.Value, name);

        await SendRoomMessageAsync(sessionUnit.Destination, x => new CmdContentInfo()
        {
            Cmd = MessageKeyNames.UpdateRoomName,
            Text = new TextTemplate("{operator} 更新群名称:'{name}'")
                    .WithData("operator", new SessionUnitTextTemplate(sessionUnit))
                    .WithData("name", new SessionUnitTextTemplate(x.Id, entity.Name))
                    .ToString(),
        });
        return entity;
    }

    public async Task<ChatObject> UpdatePortraitAsync(SessionUnit sessionUnit, string thumbnail, string portrait, Action<long> beforeSend = null)
    {
        var entity = await ChatObjectManager.UpdateAsync(
            sessionUnit.DestinationId.Value,
            x => x.SetPortrait(thumbnail, portrait),
            isUnique: false);

        //beforeSend?.Invoke(entity.Id);

        await SendRoomMessageAsync(sessionUnit.Destination, x => new CmdContentInfo()
        {
            Cmd = MessageKeyNames.UpdatePortrait,
            Text = new TextTemplate("{operator} 更新群头像:'{room}'")
                    .WithData("operator", new SessionUnitTextTemplate(sessionUnit))
                    .WithData("room", new SessionUnitTextTemplate(x.Id, entity.Name))
                    .ToString(),
        });
        return entity;
    }



    public async Task TransferCreatorAsync(Guid sessionUnitId, Guid targetSessionUnitId, bool isSendMessageToRoom = true)
    {
        var sessionUnit = await SessionUnitRepository.GetAsync(sessionUnitId);

        Assert.If(!sessionUnit.Setting.IsCreator, "Not the session creator");

        var targetSessionUnit = await SessionUnitRepository.GetAsync(targetSessionUnitId);

        Assert.If(sessionUnit.SessionId != targetSessionUnit.SessionId, "Not in same session");

        sessionUnit.Setting.SetIsCreator(false);

        targetSessionUnit.Setting.SetIsCreator(true);

        if (isSendMessageToRoom)
        {
            await SendRoomMessageAsync(sessionUnit.Destination, new CmdContentInfo()
            {
                Cmd = MessageKeyNames.TransferCreator,
                Text = new TextTemplate("{operator} 转让群,{targetObject} 成为群主")
                    .WithData("operator", new SessionUnitTextTemplate(sessionUnit))
                    .WithData("targetObject", new SessionUnitTextTemplate(targetSessionUnit))
                    .ToString(),
            });
        }
    }
}
