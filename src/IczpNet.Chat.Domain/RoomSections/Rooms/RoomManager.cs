using Microsoft.EntityFrameworkCore;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;
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
    protected Type ObjectMapperContext { get; set; }
    protected IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetService<IObjectMapper>(provider =>
        ObjectMapperContext == null
            ? provider.GetRequiredService<IObjectMapper>()
            : (IObjectMapper)provider.GetRequiredService(typeof(IObjectMapper<>).MakeGenericType(ObjectMapperContext)));

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
        var current = UnitOfWorkManager.Current;
        if (current?.Options.IsTransactional != true)
        {
            using var transactional = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
            var created = await CreateInTransactionAsync(name, memberIdList, ownerId, transactional);
            await transactional.CompleteAsync();
            return created;
        }

        return await CreateInTransactionAsync(name, memberIdList, ownerId, current);
    }

    private async Task<ChatObject> CreateInTransactionAsync(string name, List<long> memberIdList, long? ownerId, IUnitOfWork uow)
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

        var content = new CmdContentInfo()
        {
            //Text = $"{roomOwner?.Name} 创建群聊'{room.Name}',{vtext}等 {_memberIdList.Count} 人加入群聊。",
            Cmd = MessageKeyNames.JoinRoom,
            Text = new TextTemplate("{creatorText} {membersJoinText}等 {count} 人加入群聊")
                        .WithData("creatorText", creatorText)
                        .WithData("membersJoinText", membersJoinText)
                        .WithData("count", _memberIdList.Count)
                        .ToString(),
        };

        // Capture data while the creating scope is alive; publish only after SQL commits.
        var units = insertSessionUnitList.Select(x => ObjectMapper.Map<SessionUnit, SessionUnitCacheItem>(x)).ToList();
        var roomUnit = units.Single(x => x.Id == roomSessionUnit.Id);
        await PrepareUnitCachesAsync(uow, units);
        await OnlineManager.AddSessionInTransactionAsync(units.Select(x => (x.SessionId!.Value, x.OwnerId)).ToList(), uow);
        var clientMessageId = "RC" + Convert.ToBase64String(session.Id.ToByteArray()).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        await LazyServiceProvider.LazyGetRequiredService<IMessageManager>()
            .CreateRoomCommandInCurrentTransactionAsync(roomUnit, content, clientMessageId);

        return room;
    }

    private async Task PrepareUnitCachesAsync(IUnitOfWork uow, List<SessionUnitCacheItem> units)
    {
        var scopeFactory = LazyServiceProvider.LazyGetRequiredService<IServiceScopeFactory>();
        // Failed is synchronous and is raised on disposal after the transaction is disposed.
        // A post-commit callback can fail too; never remove cache for a committed unit of work.
        uow.Failed += (_, _) =>
        {
            if (uow.IsCompleted) return;
            try
            {
                using var scope = scopeFactory.CreateScope();
                scope.ServiceProvider.GetRequiredService<ISessionUnitCacheManager>()
                    .RemoveNewUnitsAsync(units).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to remove rolled-back room unit caches for session {SessionId}", units.FirstOrDefault()?.SessionId);
            }
        };
        await SessionUnitCacheManager.AddUnitsAsync(units);
    }

    public virtual async Task<List<SessionUnit>> InviteAsync(InviteInput input, bool autoSendMessage = true)
    {
        var current = UnitOfWorkManager.Current;
        if (current?.Options.IsTransactional != true)
        {
            using var transactional = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
            var invited = await InviteInTransactionAsync(input, autoSendMessage, transactional);
            await transactional.CompleteAsync();
            return invited;
        }

        return await InviteInTransactionAsync(input, autoSendMessage, current);
    }

    private async Task<List<SessionUnit>> InviteInTransactionAsync(InviteInput input, bool autoSendMessage, IUnitOfWork uow)
    {
        //var room = await GetWalletAsync(roomId);

        var session = await SessionManager.GetByOwnerIdAsync(input.RoomId);

        SessionUnit inviterSessionUnit = null;

        if (input.InviterId.HasValue)
        {
            inviterSessionUnit = await (await SessionUnitRepository.GetQueryableAsync())
                .Include(x => x.Setting)
                .FirstOrDefaultAsync(x => x.SessionId == session.Id && x.OwnerId == input.InviterId.Value);
            Assert.If(inviterSessionUnit == null, "邀请人不在群里");

            var isManager = inviterSessionUnit.Setting != null
                && (inviterSessionUnit.Setting.IsCreator || inviterSessionUnit.Setting.IsAdmin);
            if (!isManager && (!input.JoinWay.HasValue || input.JoinWay == JoinWays.Invitation))
            {
                if (!session.AllowJoinChannels.HasFlag(RoomJoinChannels.Invitation))
                {
                    throw new Volo.Abp.UserFriendlyException("当前群已关闭成员邀请入群");
                }
                if (session.InvitePolicy == RoomInvitePolicies.AdminsOnly)
                {
                    throw new Volo.Abp.UserFriendlyException("当前群仅允许群主和管理员邀请成员");
                }
                if (session.InvitePolicy == RoomInvitePolicies.ApprovalRequired)
                {
                    throw new Volo.Abp.UserFriendlyException("当前群已开启邀请需管理员确认，请通过入群申请加入");
                }
            }
        }

        var queryable = await SessionUnitRepository.GetQueryableAsync();
        var existingUnits = await AsyncExecuter.ToListAsync(
            queryable
                .Include(x => x.Setting)
                .Where(x => x.SessionId == session.Id && input.MemberIdList.Contains(x.OwnerId)));

        // 已经在群里且处于活跃状态（未退群未删除）的成员
        var activeInMemberList = existingUnits
            .Where(x => x.Setting != null && !x.Setting.IsKilled && !x.IsDeleted)
            .Select(x => x.OwnerId)
            .ToList();

        Assert.If(activeInMemberList.Any(), $"有 {activeInMemberList.Count} 人已经在群里,[id:{activeInMemberList.FirstOrDefault()}]");

        // 曾经在群里但退出/被踢出的成员（需要重新激活复用原有 SessionUnit.Id）
        var rejoinUnits = existingUnits
            .Where(x => (x.Setting != null && x.Setting.IsKilled) || x.IsDeleted)
            .ToList();

        var existingOwnerIds = existingUnits.Select(x => x.OwnerId).ToHashSet();
        var brandNewMemberIdList = input.MemberIdList.Where(id => !existingOwnerIds.Contains(id)).ToList();

        var allCandidateMemberIds = input.MemberIdList.Except(activeInMemberList).ToList();
        Assert.If(allCandidateMemberIds.Count == 0, "没有数据:memberIdList");

        var allJoinedMembers = await ChatObjectManager.GetManyAsync(allCandidateMemberIds);
        Assert.If(allJoinedMembers.Count == 0, "没有数据:allJoinedMembers");

        foreach (var member in allJoinedMembers)
        {
            Assert.If(!IsAllowJoinRoom(member.ObjectType.GetValueOrDefault()), $"不能加入群:[id:${member.Id},ObjectType:{member.ObjectType}]");
        }

        var room = await ChatObjectManager.GetAsync(input.RoomId);
        var disposalManager = LazyServiceProvider.LazyGetRequiredService<ISessionUnitDisposalManager>();

        // 1. 重新激活已有 SessionUnit
        foreach (var rejoinUnit in rejoinUnits)
        {
            rejoinUnit.IsDeleted = false;
            rejoinUnit.DeleterId = null;
            rejoinUnit.DeletionTime = null;
            if (rejoinUnit.Setting != null)
            {
                rejoinUnit.Setting.IsKilled = false;
                rejoinUnit.Setting.KillTime = null;
                rejoinUnit.Setting.JoinTime = DateTime.Now;
                rejoinUnit.Setting.JoinWay = input.JoinWay ?? JoinWays.Invitation;
                rejoinUnit.Setting.InviterId = inviterSessionUnit?.Id;
            }

            // 记录重新入群处置事件（Normal / MemberRejoined）通知各端恢复活跃状态
            await disposalManager.RecordDisposalAsync(
                ownerId: rejoinUnit.OwnerId,
                sessionUnitId: rejoinUnit.Id,
                sessionId: rejoinUnit.SessionId,
                destinationId: rejoinUnit.DestinationId,
                disposalType: SessionUnitDisposalType.Normal,
                reason: SessionUnitDisposalReason.MemberRejoined,
                operatorId: input.InviterId);
        }

        if (rejoinUnits.Count > 0)
        {
            await SessionUnitRepository.UpdateManyAsync(rejoinUnits, autoSave: true);
        }

        // 2. 为全新成员创建 SessionUnit
        var newJoinMembers = allJoinedMembers.Where(m => brandNewMemberIdList.Contains(m.Id)).ToList();
        var newMemberSessionUnitList = newJoinMembers.Select(x =>
            SessionUnitManager.Generate(
                session: session,
                owner: x,
                destination: room,
                x =>
                {
                    x.JoinWay = input.JoinWay ?? JoinWays.Invitation;
                    x.InviterId = inviterSessionUnit?.Id;
                }))
            .ToList();

        if (newMemberSessionUnitList.Count > 0)
        {
            await SessionUnitRepository.InsertManyAsync(newMemberSessionUnitList, autoSave: true);
        }

        var joinMemberSessionUnitList = rejoinUnits.Concat(newMemberSessionUnitList).ToList();
        var units = joinMemberSessionUnitList.Select(x => ObjectMapper.Map<SessionUnit, SessionUnitCacheItem>(x)).ToList();
        SessionUnitCacheItem roomUnit = null;
        CmdContentInfo content = null;
        if (autoSendMessage)
        {
            var inviterText = string.Empty;

            if (input.InviterId != null)
            {
                var inviter = await ChatObjectManager.GetAsync(input.InviterId.Value);

                inviterText = new TextTemplate("{inviterObject} 邀请 ")
                                .WithData("inviterObject", new SessionUnitTextTemplate(inviterSessionUnit))
                                .ToString();
            }

            var membersJoinText = joinMemberSessionUnitList.Take(3)
                                .Select(x => new SessionUnitTextTemplate(x.Id, allJoinedMembers.FirstOrDefault(d => d.Id == x.OwnerId)?.Name).ToString())
                                .JoinAsString("、");

            content = new CmdContentInfo()
            {
                Cmd = MessageKeyNames.JoinRoom,
                //Text = $"{inviterText}{joinMembers.Select(x => x.Name).JoinAsString("、")}等 {joinMembers.Count} 人加入群聊。",
                Text = new TextTemplate("{inviterText} {membersJoinText} 等 {count} 人加入群聊")
                            .WithData("inviterText", inviterText)
                            .WithData("membersJoinText", membersJoinText)
                            .WithData("count", allJoinedMembers.Count)
                            .ToString(),
            };
            var senderUnit = await SessionUnitManager.FindAsync(room.Id, room.Id);
            if (senderUnit == null)
            {
                senderUnit = AddRoomSessionUnit(session, room);
                await uow.SaveChangesAsync();
            }
            roomUnit = ObjectMapper.Map<SessionUnit, SessionUnitCacheItem>(senderUnit);
        }

        await PrepareUnitCachesAsync(uow, units);
        await OnlineManager.AddSessionInTransactionAsync(units.Select(x => (x.SessionId!.Value, x.OwnerId)).ToList(), uow);
        if (content != null)
        {
            var clientMessageId = "RI" + Convert.ToBase64String(units[0].Id.ToByteArray()).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            await LazyServiceProvider.LazyGetRequiredService<IMessageManager>()
                .CreateRoomCommandInCurrentTransactionAsync(roomUnit, content, clientMessageId);
        }
        return joinMemberSessionUnitList;
    }

    protected virtual Task SendRoomMessageAsync(SessionUnit roomSessionUnit, CmdContentInfo content)
    {
        Assert.If(roomSessionUnit.OwnerObjectType != ChatObjectTypeEnums.Room, $"Fail ObjectType:{roomSessionUnit.OwnerObjectType}");

        var unit = ObjectMapper.Map<SessionUnit, SessionUnitCacheItem>(roomSessionUnit);

        return MessageSender.SendCmdAsync(unit, new MessageInput<CmdContentInfo>()
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
