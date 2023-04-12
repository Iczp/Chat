using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.OfficialSections.Officials
{
    public class OfficialManager : ChatObjectManager, IOfficialManager
    {
        protected ISessionUnitManager SessionUnitManager { get; }

        public OfficialManager(IChatObjectRepository repository,

            ISessionUnitManager sessionUnitManager) : base(repository)
        {
            SessionUnitManager = sessionUnitManager;
        }


        protected override async Task CheckExistsByCreateAsync(ChatObject inputEntity)
        {
            Assert.If(await Repository.AnyAsync(x => x.ObjectType == inputEntity.ObjectType && x.Name == inputEntity.Name), $"Already exists name:{inputEntity.Name},ObjectType:{inputEntity.ObjectType}");
        }

        protected override async Task CheckExistsByUpdateAsync(ChatObject inputEntity)
        {
            Assert.If(await Repository.AnyAsync((x) => x.ObjectType == inputEntity.ObjectType && x.Name == inputEntity.Name && !x.Id.Equals(inputEntity.Id)), $" Name[{inputEntity.Name}] already such,,ObjectType:{inputEntity.ObjectType}");
        }


        private SessionUnit AddOfficialSessionUnit(Session session, long officialId)
        {
            return session.AddSessionUnit(new SessionUnit(
                  id: GuidGenerator.Create(),
                  session: session,
                  ownerId: officialId,
                  destinationId: officialId,
                  destinationObjectType: ChatObjectTypeEnums.Official,
                  isPublic: false,
                  isStatic: true,
                  isCreator: true,
                  joinWay: JoinWays.System,
                  inviterUnitId: null,
                  isInputEnabled: true));
        }

        public override async Task<ChatObject> CreateAsync(ChatObject inputEntity, bool isUnique = true)
        {
            var official = await base.CreateAsync(inputEntity, isUnique);

            var session = await SessionGenerator.MakeAsync(official, official);

            session.SetOwner(official);

            AddOfficialSessionUnit(session, official.Id);

            // commit to db
            await CurrentUnitOfWork.SaveChangesAsync();

            return official;
        }

        public async Task<SessionUnit> SubscribeAsync(long ownerId, long destinationId)
        {
            var sessionUnit = await SessionUnitManager.FindAsync(ownerId, destinationId);

            //Unsubscribed
            if (sessionUnit == null)
            {
                var owner = await GetAsync(ownerId);

                var official = await GetAsync(destinationId);

                Assert.If(official.ObjectType != ChatObjectTypeEnums.Official, $"ObjectType must be '{ChatObjectTypeEnums.Official}',destinationId:{destinationId}");

                var session = await SessionGenerator.MakeAsync(owner, official);

                sessionUnit = session.AddSessionUnit(new SessionUnit(
                  id: GuidGenerator.Create(),
                  session: session,
                  ownerId: ownerId,
                  destinationId: official.Id,
                  destinationObjectType: ChatObjectTypeEnums.Official,
                  isPublic: true,
                  isStatic: false,
                  isCreator: false,
                  joinWay: JoinWays.Normal,
                  inviterUnitId: null,
                  isInputEnabled: false));
            }
            else
            {
                //Assert.If(sessionUnit.IsEnabled, $"Already enabled,IsEnabled:{sessionUnit.IsEnabled}");
            }
            sessionUnit.SetIsEnabled(true);

            await CurrentUnitOfWork.SaveChangesAsync();

            await SendMessageAsync(sessionUnit, "启动成功");

            return sessionUnit;
        }

        private async Task SendMessageAsync(SessionUnit receiverSessionUnit, string text)
        {
            var officialSessionUnit = await SessionUnitManager.FindAsync(receiverSessionUnit.DestinationId.Value, receiverSessionUnit.DestinationId.Value);
            await MessageSender.SendCmdAsync(
                senderSessionUnit: officialSessionUnit,
                input: new MessageSendInput<CmdContentInfo>()
                {
                    SessionUnitId = officialSessionUnit.Id,
                    Content = new CmdContentInfo()
                    {
                        Text = text
                    }
                },
                receiverSessionUnit: receiverSessionUnit);
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

            Assert.If(sessionUnit.IsEnabled == isEnabled, $"Unchanged IsEnabled:{isEnabled}");

            sessionUnit.SetIsEnabled(isEnabled);

            await CurrentUnitOfWork.SaveChangesAsync();

            await SendMessageAsync(sessionUnit, isEnabled ? "启动成功" : "禁用成功");

            return sessionUnit;
        }
    }
}
