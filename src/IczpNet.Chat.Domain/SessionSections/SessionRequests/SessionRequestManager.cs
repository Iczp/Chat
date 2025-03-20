using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionSections.SessionPermissionUnitGrants;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.Settings;
using IczpNet.Chat.TextTemplates;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Settings;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionSections.SessionRequests;

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
        ISessionPermissionChecker sessionPermissionChecker)
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
            .Where(x => x.ExpirationTime == null || x.ExpirationTime > DateTime.Now)
            .Where(x => x.OwnerId == ownerId && x.DestinationId == destinationId)
            .FirstOrDefault();

        var expiractionHours = await SettingProvider.GetAsync(ChatSettings.SessionRequestExpirationHours, defaultValue: 72);

        var destination = await ChatObjectManager.GetAsync(destinationId);

        if (entity != null)
        {
            entity.RequestMessage = requestMessage;

            entity.SetExpirationTime(expiractionHours);

            await Repository.UpdateAsync(entity, autoSave: true);
        }
        else
        {
            await CheckMaxSessionUnitCountAsync(owner.Id);

            entity = new SessionRequest(GuidGenerator.Create(), owner, destination, requestMessage);

            entity.SetExpirationTime(expiractionHours);

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
                await SendForPersonalAsync(owner, destination, entity);
                break;
            case ChatObjectTypeEnums.Room:
            case ChatObjectTypeEnums.Square:
                await SendForRoomOrSquareAsync(owner, destination, entity);
                break;
            default:
                Assert.If(true, $"The owner's ObjectType '{owner.ObjectType}' disallow create session request. OwnerId:{ownerId}");
                break;
        }

        return entity;
    }

    protected virtual async Task SendForRoomOrSquareAsync(ChatObject owner, ChatObject destination, SessionRequest sessionRequest)
    {
        var assistant = await ChatObjectManager.GetOrAddPrivateAssistantAsync();

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
            await SessionGenerator.MakeAsync(assistant, managerOrCretor);

            var senderSessionUnit = await SessionUnitManager.FindAsync(assistant.Id, managerOrCretor.Id);

            await MessageSender.SendLinkAsync(senderSessionUnit, new MessageInput<LinkContentInfo>()
            {
                Content = new LinkContentInfo()
                {
                    Title = new TextTemplate("{owner} 加入 '{destination}'")
                                    .WithData("owner", new ChatObjectTextTemplate(sessionRequest.Owner))
                                    .WithData("destination", new ChatObjectTextTemplate(sessionRequest.Destination))
                                    .ToString(),
                    Url = $"app://sesson-request/detail?id={sessionRequest.Id}"
                }
            });
        }
    }

    protected virtual async Task SendForPersonalAsync(ChatObject owner, ChatObject receiver, SessionRequest sessionRequest)
    {
        var assistant = await ChatObjectManager.GetOrAddPrivateAssistantAsync();

        await SessionGenerator.MakeAsync(assistant, receiver);

        var sessionUnit = await SessionUnitManager.FindAsync(assistant.Id, receiver.Id);

        await MessageSender.SendLinkAsync(sessionUnit, new MessageInput<LinkContentInfo>()
        {
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
        Logger.LogInformation($"HandleRequest:sessionRequestId={sessionRequestId},isAgreed={isAgreed},handlMessage={handlMessage},handlerSessionUnitId={handlerSessionUnitId}");

        var stopwatch = Stopwatch.StartNew();

        var sessionRequest = await Repository.GetAsync(sessionRequestId);

        Assert.If(sessionRequest.IsHandled, $"Already been handled:IsAgreed={sessionRequest.IsAgreed}");

        var ownerSessionUnit = await SessionUnitManager.FindAsync(sessionRequest.OwnerId, sessionRequest.DestinationId.Value);

        if (isAgreed)
        {
            await CheckMaxSessionUnitCountAsync(sessionRequest.DestinationId.Value);

            sessionRequest.AgreeRequest(handlMessage, handlerSessionUnitId);

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

                    var destinationSessionUnit = await SessionUnitManager.CreateIfNotContainsAsync(
                          session: session,
                          owner: sessionRequest.Destination,
                          destination: sessionRequest.Owner,
                          x =>
                          {
                              x.JoinWay = JoinWays.Request;
                          });

                    await MessageSender.SendCmdAsync(destinationSessionUnit, new MessageInput<CmdContentInfo>()
                    {
                        Content = new CmdContentInfo()
                        {
                            Text = $"添加好友成功",
                        }
                    });

                    break;
                case ChatObjectTypeEnums.Room:
                case ChatObjectTypeEnums.Square:

                    if (handlerSessionUnitId.HasValue)
                    {
                        var handlerSessionUnit = await SessionUnitManager.GetAsync(handlerSessionUnitId.Value);
                        await SessionPermissionChecker.CheckAsync(SessionPermissionDefinitionConsts.SessionRequestPermission.Handle, handlerSessionUnit);
                    }

                    var roomOrSquareSessionUnit = await SessionUnitManager.CreateIfNotContainsAsync(
                             session: session,
                             owner: sessionRequest.Destination,
                             destination: sessionRequest.Destination,
                             x =>
                             {
                                 x.JoinWay = JoinWays.Request;
                             });

                    await MessageSender.SendCmdAsync(roomOrSquareSessionUnit, new MessageInput<CmdContentInfo>()
                    {
                        Content = new CmdContentInfo()
                        {
                            Text = new TextTemplate("欢迎 '{owner}' 加入 '{destination}'")
                                    .WithData("owner", new SessionUnitTextTemplate(ownerSessionUnit.Id, sessionRequest.Owner.Name))
                                    .WithData("destination", new ChatObjectTextTemplate(sessionRequest.Destination))
                                    .ToString(),
                        }
                    });
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

                await SendForRequesterAsync(sessionRequest.Owner, sessionRequest);
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


    protected virtual async Task SendForRequesterAsync(IChatObject receiver, SessionRequest sessionRequest)
    {
        var assistant = await ChatObjectManager.GetOrAddPrivateAssistantAsync();

        await SessionGenerator.MakeAsync(assistant, sessionRequest.Owner);

        var sessionUnit = await SessionUnitManager.FindAsync(assistant.Id, sessionRequest.Owner.Id);

        await MessageSender.SendLinkAsync(sessionUnit, new MessageInput<LinkContentInfo>()
        {
            Content = new LinkContentInfo()
            {
                //Title = $"'{Destination}'拒绝'{sessionRequest.Owner.Name}'请求:{sessionRequest.HandleMessage}",
                Title = new TextTemplate("'{destination}' 拒绝 '{Owner}' 请求:{handleMessage}")
                            .WithData("destination", new ChatObjectTextTemplate(sessionRequest.Destination))
                            .WithData("owner", new ChatObjectTextTemplate(sessionRequest.Owner))
                            .WithData("handleMessage", sessionRequest.HandleMessage)
                            .ToString(),
                Url = $"app://sesson-request/detail?id={sessionRequest.Id}"
            }
        });
    }
}
