using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionPermissions;
using IczpNet.Chat.SessionPermissionUnitGrants;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.Settings;
using IczpNet.Chat.TextTemplates;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Settings;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionRequests;

public class SessionRequestManager : DomainService, ISessionRequestManager
{
    protected IRepository<SessionRequest, Guid> Repository { get; }
    protected ISessionUnitRepository SessionUnitRepository { get; }
    protected ISessionUnitManager SessionUnitManager { get; }
    protected ISessionManager SessionManager { get; }
    protected IMessageSender MessageSender { get; }
    protected IChatObjectManager ChatObjectManager { get; }
    protected ISessionGenerator SessionGenerator { get; }
    protected IRepository<SessionPermissionRoleGrant> SessionPermissionRoleGrantRepository { get; }
    protected IRepository<SessionPermissionUnitGrant> SessionPermissionUnitGrantRepository { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected ISettingProvider SettingProvider { get; }
    protected ISessionPermissionChecker SessionPermissionChecker { get; }
    protected IBackgroundJobManager BackgroundJobManager { get; }
    protected List<ChatObjectTypeEnums> DisallowCreateList { get; set; } = new List<ChatObjectTypeEnums>() {
        ChatObjectTypeEnums.Robot,
        ChatObjectTypeEnums.Anonymous,
        ChatObjectTypeEnums.Room,
        ChatObjectTypeEnums.Square,
    };

    public SessionRequestManager(IRepository<SessionRequest, Guid> repository,
        ISessionUnitRepository sessionUnitRepository,
        ISessionUnitManager sessionUnitManager,
        ISessionManager sessionManager,
        IMessageSender messageSender,
        IChatObjectManager chatObjectManager,
        ISessionGenerator sessionGenerator,
        IRepository<SessionPermissionRoleGrant> sessionPermissionRoleGrantRepository,
        IRepository<SessionPermissionUnitGrant> sessionPermissionUnitGrantRepository,
        IUnitOfWorkManager unitOfWorkManager,
        ISettingProvider settingProvider,
        ISessionPermissionChecker sessionPermissionChecker,
        IBackgroundJobManager backgroundJobManager)
    {
        Repository = repository;
        SessionUnitRepository = sessionUnitRepository;
        SessionUnitManager = sessionUnitManager;
        SessionManager = sessionManager;
        MessageSender = messageSender;
        ChatObjectManager = chatObjectManager;
        SessionGenerator = sessionGenerator;
        SessionPermissionRoleGrantRepository = sessionPermissionRoleGrantRepository;
        SessionPermissionUnitGrantRepository = sessionPermissionUnitGrantRepository;
        UnitOfWorkManager = unitOfWorkManager;
        SettingProvider = settingProvider;
        SessionPermissionChecker = sessionPermissionChecker;
        BackgroundJobManager = backgroundJobManager;
    }

    protected virtual async Task CheckMaxSessionUnitCountAsync(long ownerId)
    {
        var maxSessionUnitCount = await SettingProvider.GetAsync<int>(ChatSettings.MaxSessionUnitCount);

        var sessionUnitCount = await SessionUnitManager.GetCountByOwnerIdAsync(ownerId);

        Assert.If(sessionUnitCount > maxSessionUnitCount, $"maxSessionUnitCount:{maxSessionUnitCount}");
    }

    public virtual async Task<SessionRequest> CreateRequestAsync(long ownerId, long destinationId, string requestMessage)
    {
        Assert.If(await SessionUnitManager.FindAsync(ownerId, destinationId) != null, "Already a friend");

        var owner = await ChatObjectManager.GetAsync(ownerId);

        Assert.If(DisallowCreateList.Contains(owner.ObjectType.Value), $"The owner's ObjectType '{owner.ObjectType}' disallow create session request. OwnerId:{ownerId}");

        var entity = (await Repository.GetQueryableAsync())
            .Where(x => !x.IsHandled && x.IsEnabled)
            .Where(x => x.ExpirationTime == null || x.ExpirationTime > Clock.Now)
            .Where(x => x.OwnerId == ownerId && x.DestinationId == destinationId)
            .FirstOrDefault();

        var expiractionHours = await SettingProvider.GetAsync(ChatSettings.SessionRequestExpirationHours, defaultValue: 72);

        var destination = await ChatObjectManager.GetAsync(destinationId);

        if (entity != null)
        {
            entity.RequestMessage = requestMessage;

            entity.SetExpirationTime(expiractionHours, Clock.Now);

            await Repository.UpdateAsync(entity, autoSave: true);
        }
        else
        {
            await CheckMaxSessionUnitCountAsync(owner.Id);

            entity = new SessionRequest(GuidGenerator.Create(), owner, destination, requestMessage);

            entity.SetExpirationTime(expiractionHours, Clock.Now);

            entity = await Repository.InsertAsync(entity, autoSave: true);
        }

        //VerificationMethods is none
        if (destination.VerificationMethod == VerificationMethods.None)
        {
            Logger.LogWarning("[Automatic handling] VerificationMethod:VerificationMethods.None");

            await HandleRequestAsync(entity.Id, true, "VerificationMethods.None", null);

            return entity;
        }

        switch (destination.ObjectType)
        {
            case ChatObjectTypeEnums.Personal:
            case ChatObjectTypeEnums.Customer:
            case ChatObjectTypeEnums.ShopKeeper:
            case ChatObjectTypeEnums.Room:
            case ChatObjectTypeEnums.Square:
                await QueueNotificationAsync(entity.Id, SessionRequestNotificationKind.Requested);
                break;
            default:
                Assert.If(true, $"The owner's ObjectType '{owner.ObjectType}' disallow create session request. OwnerId:{ownerId}");
                break;
        }

        return entity;
    }

    protected virtual async Task SendForRoomOrSquareAsync(ChatObject owner, ChatObject destination, SessionRequest sessionRequest)
    {
        // room creator and room manager

        var roleIdList = (await SessionPermissionRoleGrantRepository.GetQueryableAsync())
              .Where(x => x.DefinitionId == SessionPermissionDefinitionConsts.SessionRequestPermission.Handle && x.IsEnabled)
              .Select(x => x.RoleId);

        var sessionUnitIdList = (await SessionPermissionUnitGrantRepository.GetQueryableAsync())
               .Where(x => x.DefinitionId == SessionPermissionDefinitionConsts.SessionRequestPermission.Handle && x.IsEnabled)
               .Select(x => x.SessionUnitId);

        var managerAndCretorList = (await SessionUnitRepository.GetQueryableAsync())
            .Where(x => x.DestinationId == destination.Id)
            .Where(x => x.SessionUnitRoleList.Any(d => roleIdList.Contains(d.SessionRoleId)) || sessionUnitIdList.Contains(x.Id) || x.Setting.IsCreator)
            .Select(x => x.Owner)
            .Distinct()
            .ToList();

        if (!managerAndCretorList.Any())
        {
            Logger.LogWarning("No manager or creator found,");
        }

        foreach (var managerOrCretor in managerAndCretorList)
        {
            var (assistant, notificationDestinationId) = await EnsureAssistantSessionCommittedAsync(managerOrCretor);

            var senderSessionUnit = await GetSenderUnitAsync(assistant.Id, notificationDestinationId);

            await MessageSender.SendLinkAsync(senderSessionUnit, new MessageInput<LinkContentInfo>()
            {
                ClientMessageId = SessionRequestNotificationArgs.GetClientMessageId(sessionRequest.Id, SessionRequestNotificationKind.Requested),
                Content = new LinkContentInfo()
                {
                    Title = new TextTemplate("{owner} 加入 '{destination}'")
                                    .WithData("owner", new ChatObjectTextTemplate(owner))
                                    .WithData("destination", new ChatObjectTextTemplate(destination))
                                    .ToString(),
                    Url = $"app://sesson-request/detail?id={sessionRequest.Id}"
                }
            });
        }
    }

    protected virtual async Task SendForPersonalAsync(ChatObject owner, ChatObject receiver, SessionRequest sessionRequest)
    {
        var (assistant, notificationDestinationId) = await EnsureAssistantSessionCommittedAsync(receiver);

        var sessionUnit = await GetSenderUnitAsync(assistant.Id, notificationDestinationId);

        await MessageSender.SendLinkAsync(sessionUnit, new MessageInput<LinkContentInfo>()
        {
            ClientMessageId = SessionRequestNotificationArgs.GetClientMessageId(sessionRequest.Id, SessionRequestNotificationKind.Requested),
            Content = new LinkContentInfo()
            {
                Title = new TextTemplate("{owner} 请求加为好友")
                            .WithData("owner", new ChatObjectTextTemplate(owner))
                            .ToString(),
                Url = $"app://sesson-request/detail?id={sessionRequest.Id}"
            }
        });
    }

    public virtual async Task<SessionRequest> HandleRequestAsync(Guid sessionRequestId, bool isAgreed, string handlMessage, Guid? handlerSessionUnitId)
    {
        Logger.LogInformation("HandleRequest:sessionRequestId={SessionRequestId},isAgreed={IsAgreed},handlerSessionUnitId={HandlerSessionUnitId}",
            sessionRequestId, isAgreed, handlerSessionUnitId);

        var stopwatch = Stopwatch.StartNew();

        var sessionRequest = await Repository.GetAsync(sessionRequestId);

        Assert.If(sessionRequest.IsHandled, $"Already been handled:IsAgreed={sessionRequest.IsAgreed}");

        var ownerSessionUnit = await SessionUnitManager.FindAsync(sessionRequest.OwnerId, sessionRequest.DestinationId.Value);

        if (isAgreed)
        {
            await CheckMaxSessionUnitCountAsync(sessionRequest.DestinationId.Value);

            sessionRequest.AgreeRequest(handlMessage, handlerSessionUnitId);
            sessionRequest.SetHandleTime(Clock.Now);

            var session = await SessionGenerator.MakeAsync(sessionRequest.Owner, sessionRequest.Destination);

            ownerSessionUnit ??= await SessionUnitManager.CreateIfNotContainsAsync(
                  session: session,
                  owner: sessionRequest.Owner,
                  destination: sessionRequest.Destination,
                  x =>
                  {
                      x.JoinWay = JoinWays.Request;
                  });

            switch (sessionRequest.Destination.ObjectType)
            {
                case ChatObjectTypeEnums.Robot:
                case ChatObjectTypeEnums.Personal:
                case ChatObjectTypeEnums.Customer:
                case ChatObjectTypeEnums.ShopKeeper:

                    // A ShopWaiter shares its parent's session. The destination
                    // may already have a unit in that session addressed to the
                    // ShopKeeper, and OwnerId+SessionId is unique.
                    if (sessionRequest.Owner.ObjectType != ChatObjectTypeEnums.ShopWaiter ||
                        await SessionUnitManager.FindBySessionIdAsync(session.Id, sessionRequest.Destination.Id) == null)
                    {
                        await SessionUnitManager.CreateIfNotContainsAsync(
                              session: session,
                              owner: sessionRequest.Destination,
                              destination: sessionRequest.Owner,
                              x =>
                              {
                                  x.JoinWay = JoinWays.Request;
                              });
                    }

                    await QueueNotificationAsync(sessionRequest.Id, SessionRequestNotificationKind.Accepted);

                    break;
                case ChatObjectTypeEnums.Room:
                case ChatObjectTypeEnums.Square:

                    if (handlerSessionUnitId.HasValue)
                    {
                        var handlerSessionUnit = await SessionUnitManager.GetAsync(handlerSessionUnitId.Value);
                        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.SessionRequestPermission.Handle, handlerSessionUnit);
                    }

                    await SessionUnitManager.CreateIfNotContainsAsync(
                             session: session,
                             owner: sessionRequest.Destination,
                             destination: sessionRequest.Destination,
                             x =>
                             {
                                 x.JoinWay = JoinWays.Request;
                             });
                    await QueueNotificationAsync(sessionRequest.Id, SessionRequestNotificationKind.Accepted);
                    break;
                default:
                    Assert.If(true, $"The destination's ObjectType '{sessionRequest.Destination.ObjectType}' disallow create session request. destinationId:{sessionRequest.Destination.Id}");
                    break;
            }
        }
        else
        {
            if (ownerSessionUnit == null)
            {
                sessionRequest.DisagreeRequest(handlMessage, handlerSessionUnitId);
                sessionRequest.SetHandleTime(Clock.Now);

                await QueueNotificationAsync(sessionRequest.Id, SessionRequestNotificationKind.Declined);
            }
            else
            {
                sessionRequest.SetIsEnabled(false);
            }
        }

        stopwatch.Stop();

        Logger.LogInformation($"HandleRequest completed:sessionRequestId={sessionRequestId}, stopwatch:{stopwatch.ElapsedMilliseconds}ms.");

        return sessionRequest;
    }


    protected virtual Task QueueNotificationAsync(Guid requestId, SessionRequestNotificationKind kind) =>
        BackgroundJobManager.EnqueueAsync(
            new SessionRequestNotificationArgs { RequestId = requestId, Kind = kind },
            delay: TimeSpan.FromSeconds(2));

    // Quartz persists the notification before the caller commits Chat SQL. A retry may
    // therefore arrive early: never send until the request's committed state is visible.
    public virtual async Task DeliverNotificationAsync(SessionRequestNotificationArgs args)
    {
        var sessionRequest = await Repository.GetAsync(args.RequestId);
        if (!sessionRequest.IsEnabled) return;

        if (args.Kind == SessionRequestNotificationKind.Requested)
        {
            if (sessionRequest.IsHandled) return;
        }
        else if (!sessionRequest.IsHandled || sessionRequest.IsAgreed !=
                 (args.Kind == SessionRequestNotificationKind.Accepted))
        {
            throw new InvalidOperationException($"Session request {args.RequestId} has not committed the expected {args.Kind} state yet.");
        }

        var owner = await ChatObjectManager.GetAsync(sessionRequest.OwnerId);
        var destination = await ChatObjectManager.GetAsync(sessionRequest.DestinationId
            ?? throw new InvalidOperationException($"Session request {args.RequestId} has no destination."));

        switch (args.Kind)
        {
            case SessionRequestNotificationKind.Requested:
                if (destination.ObjectType is ChatObjectTypeEnums.Room or ChatObjectTypeEnums.Square)
                    await SendForRoomOrSquareAsync(owner, destination, sessionRequest);
                else
                    await SendForPersonalAsync(owner, destination, sessionRequest);
                break;
            case SessionRequestNotificationKind.Accepted:
                var sender = await GetSenderUnitAsync(destination.Id,
                    destination.ObjectType is ChatObjectTypeEnums.Room or ChatObjectTypeEnums.Square
                        ? destination.Id
                        : owner.ObjectType == ChatObjectTypeEnums.ShopWaiter && owner.ParentId.HasValue
                            ? owner.ParentId.Value : owner.Id);
                var text = "添加好友成功";
                if (destination.ObjectType is ChatObjectTypeEnums.Room or ChatObjectTypeEnums.Square)
                {
                    var ownerUnit = await SessionUnitManager.FindAsync(owner.Id, destination.Id)
                        ?? throw new InvalidOperationException($"Owner session unit for request {args.RequestId} is not committed yet.");
                    text = new TextTemplate("欢迎 '{owner}' 加入 '{destination}'")
                        .WithData("owner", new SessionUnitTextTemplate(ownerUnit.Id, owner.Name))
                        .WithData("destination", new ChatObjectTextTemplate(destination))
                        .ToString();
                }
                await MessageSender.SendCmdAsync(sender, new MessageInput<CmdContentInfo>
                {
                    ClientMessageId = SessionRequestNotificationArgs.GetClientMessageId(args.RequestId, args.Kind),
                    Content = new CmdContentInfo
                    {
                        Cmd = destination.ObjectType is ChatObjectTypeEnums.Room or ChatObjectTypeEnums.Square
                            ? null : MessageKeyNames.AddFriendSuccess,
                        Text = text,
                    }
                });
                break;
            case SessionRequestNotificationKind.Declined:
                await SendForRequesterAsync(owner, destination, sessionRequest);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(args.Kind), args.Kind, null);
        }
    }

    // Creating the assistant's session and sending a message cannot share an uncommitted
    // Chat transaction because MessageManager intentionally starts a new transaction.
    private async Task<(ChatObject Assistant, long NotificationDestinationId)> EnsureAssistantSessionCommittedAsync(ChatObject receiver)
    {
        ChatObject assistant;
        var notificationDestinationId = receiver.ObjectType == ChatObjectTypeEnums.ShopWaiter && receiver.ParentId.HasValue
            ? receiver.ParentId.Value
            : receiver.Id;
        using (var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
        {
            assistant = await ChatObjectManager.GetOrAddPrivateAssistantAsync();
            // Never attach the ChatObject from the caller's UOW to this new UOW.
            // EF would otherwise try to insert it again when creating a session.
            var notificationDestination = await ChatObjectManager.GetAsync(notificationDestinationId);
            await SessionGenerator.MakeAsync(assistant, notificationDestination);
            await uow.CompleteAsync();
        }
        return (assistant, notificationDestinationId);
    }

    private async Task<SessionUnitCacheItem> GetSenderUnitAsync(long ownerId, long destinationId)
    {
        // The persisted unit's computed cache fields dereference Owner and
        // Destination. Keep the query and mapping in one explicit read UOW;
        // a repository-created implicit UOW would dispose its DbContext first.
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
        var query = await SessionUnitRepository.WithDetailsAsync(
            x => x.Owner, x => x.Destination, x => x.Setting);
        var unit = await AsyncExecuter.FirstOrDefaultAsync(query.Where(
            x => x.OwnerId == ownerId && x.DestinationId == destinationId));
        if (unit is null)
            throw new InvalidOperationException($"Sender session unit {ownerId}->{destinationId} is not committed yet.");
        var item = await SessionUnitManager.MapToCacheAsync(unit);
        await uow.CompleteAsync();
        return item;
    }

    protected virtual async Task SendForRequesterAsync(ChatObject owner, ChatObject destination, SessionRequest sessionRequest)
    {
        var (assistant, notificationDestinationId) = await EnsureAssistantSessionCommittedAsync(owner);

        var sessionUnit = await GetSenderUnitAsync(assistant.Id, notificationDestinationId);

        await MessageSender.SendLinkAsync(sessionUnit, new MessageInput<LinkContentInfo>()
        {
            ClientMessageId = SessionRequestNotificationArgs.GetClientMessageId(sessionRequest.Id, SessionRequestNotificationKind.Declined),
            Content = new LinkContentInfo()
            {
                //Title = $"'{Destination}'拒绝'{sessionRequest.Owner.Name}'请求:{sessionRequest.HandleMessage}",
                Title = new TextTemplate("'{destination}' 拒绝 '{Owner}' 请求:{handleMessage}")
                            .WithData("destination", new ChatObjectTextTemplate(destination))
                            .WithData("owner", new ChatObjectTextTemplate(owner))
                            .WithData("handleMessage", sessionRequest.HandleMessage)
                            .ToString(),
                Url = $"app://sesson-request/detail?id={sessionRequest.Id}"
            }
        });
    }
}
