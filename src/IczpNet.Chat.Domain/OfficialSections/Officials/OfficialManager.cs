using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace IczpNet.Chat.OfficialSections.Officials;

public class OfficialManager : DomainService, IOfficialManager
{
    protected ISessionUnitManager SessionUnitManager { get; }
    protected IChatObjectRepository ChatObjectRepository { get; }
    protected IChatObjectManager ChatObjectManager { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected IMessageSender MessageSender { get; }
    protected ISessionGenerator SessionGenerator { get; }
    protected ISessionUnitIdGenerator SessionUnitIdGenerator { get; }
    public OfficialManager(
        IChatObjectRepository chatObjectRepository,
        ISessionUnitManager sessionUnitManager,
        IChatObjectManager chatObjectManager,
        IUnitOfWorkManager unitOfWorkManager,
        IMessageSender messageSender,
        ISessionGenerator sessionGenerator,
        ISessionUnitIdGenerator sessionUnitIdGenerator)
    {
        ChatObjectRepository = chatObjectRepository;
        SessionUnitManager = sessionUnitManager;
        ChatObjectManager = chatObjectManager;
        UnitOfWorkManager = unitOfWorkManager;
        MessageSender = messageSender;
        SessionGenerator = sessionGenerator;
        SessionUnitIdGenerator = sessionUnitIdGenerator;
    }

    protected virtual async Task CheckExistsByCreateAsync(ChatObject inputEntity)
    {
        Assert.If(await ChatObjectRepository.AnyAsync(x => x.ObjectType == inputEntity.ObjectType && x.Name == inputEntity.Name), $"Already exists name:{inputEntity.Name},ObjectType:{inputEntity.ObjectType}");
    }

    protected virtual async Task CheckExistsByUpdateAsync(ChatObject inputEntity)
    {
        Assert.If(await ChatObjectRepository.AnyAsync((x) => x.ObjectType == inputEntity.ObjectType && x.Name == inputEntity.Name && !x.Id.Equals(inputEntity.Id)), $" Name[{inputEntity.Name}] already such,,ObjectType:{inputEntity.ObjectType}");
    }

    private SessionUnit AddOfficialSessionUnit(Session session, ChatObject official)
    {
        return SessionUnitManager.Create(
              session: session,
              owner: official,
              destination: official,
              x =>
              {
                  x.IsPublic = false;
                  x.IsStatic = true;
                  x.JoinWay = JoinWays.System;
              });
    }

    public virtual async Task<ChatObject> CreateAsync(ChatObject inputEntity, bool isUnique = true)
    {
        var official = await ChatObjectManager.CreateAsync(inputEntity, isUnique);

        var session = await SessionGenerator.MakeAsync(official, official);

        session.SetOwner(official);

        AddOfficialSessionUnit(session, official);

        // commit to db
        await UnitOfWorkManager.Current.SaveChangesAsync();

        return official;
    }

    public async Task<SessionUnit> SubscribeAsync(long ownerId, long destinationId)
    {
        var sessionUnit = await SessionUnitManager.FindAsync(ownerId, destinationId);

        //Unsubscribed
        if (sessionUnit == null)
        {
            var owner = await ChatObjectManager.GetAsync(ownerId);

            var official = await ChatObjectManager.GetAsync(destinationId);

            Assert.If(official.ObjectType != ChatObjectTypeEnums.Official, $"ObjectType must be '{ChatObjectTypeEnums.Official}',destinationId:{destinationId}");

            var session = await SessionGenerator.MakeAsync(owner, official);

            sessionUnit = SessionUnitManager.Create(session: session, owner: owner, destination: official, x => x.JoinWay = JoinWays.Normal);
        }
        else
        {
            //Assert.If(sessionUnit.IsEnabled, $"Already enabled,IsEnabled:{sessionUnit.IsEnabled}");
        }
        sessionUnit.Setting.SetIsEnabled(true);

        await UnitOfWorkManager.Current.SaveChangesAsync();

        await SendMessageAsync(sessionUnit, "启动成功");

        return sessionUnit;
    }

    private async Task SendMessageAsync(SessionUnit receiverSessionUnit, string text)
    {
        var officialSessionUnit = await SessionUnitManager.FindAsync(receiverSessionUnit.DestinationId.Value, receiverSessionUnit.DestinationId.Value);
        await MessageSender.SendCmdAsync(
            senderSessionUnit: officialSessionUnit,

            input: new MessageInput<CmdContentInfo>()
            {
                //ReceiverSessionUnitId = receiverSessionUnit.Id,
                Content = new CmdContentInfo()
                {
                    Text = text
                }
            });
    }

    public Task<SessionUnit> UnsubscribeAsync(Guid sessionUnitId)
    {
        return SetIsEnabledAsync(sessionUnitId, isEnabled: false);
    }

    public Task<SessionUnit> SubscribeByIdAsync(Guid sessionUnitId)
    {
        return SetIsEnabledAsync(sessionUnitId, isEnabled: true);
    }

    protected virtual async Task<SessionUnit> SetIsEnabledAsync(Guid sessionUnitId, bool isEnabled)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

        Assert.If(sessionUnit.Setting.IsEnabled == isEnabled, $"Unchanged IsEnabled:{isEnabled}");

        sessionUnit.Setting.SetIsEnabled(isEnabled);

        await UnitOfWorkManager.Current.SaveChangesAsync();

        await SendMessageAsync(sessionUnit, isEnabled ? "启动成功" : "禁用成功");

        return sessionUnit;
    }
}
