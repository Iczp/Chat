using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionUnits;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using IczpNet.Chat.TextTemplates;

namespace IczpNet.Chat.CallCenters
{
    public class CallCenterManager : DomainService, ICallCenterManager
    {
        protected ISessionUnitManager SessionUnitManager { get; }

        protected ISessionGenerator SessionGenerator { get; }

        protected ISessionUnitIdGenerator SessionUnitIdGenerator { get; }

        protected IChatObjectManager ChatObjectManager { get; }

        protected IMessageSender MessageSender { get; }

        public CallCenterManager(ISessionUnitManager sessionUnitManager,
            ISessionGenerator sessionGenerator,
            ISessionUnitIdGenerator sessionUnitIdGenerator,
            IChatObjectManager chatObjectManager,
            IMessageSender messageSender)
        {
            SessionUnitManager = sessionUnitManager;
            SessionGenerator = sessionGenerator;
            SessionUnitIdGenerator = sessionUnitIdGenerator;
            ChatObjectManager = chatObjectManager;
            MessageSender = messageSender;
        }

        /// <inheritdoc/>
        public async Task<SessionUnit> TransferToAsync(Guid sessionUnitId, long waiterId, bool isNotice = true)
        {
            var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

            Assert.If(sessionUnit.OwnerId == waiterId, "Unable to transfer to oneself");

            var shopObjectTypes = new List<ChatObjectTypeEnums?>() { ChatObjectTypeEnums.ShopKeeper, ChatObjectTypeEnums.ShopWaiter };

            var ownerObjectType = sessionUnit.OwnerObjectType;

            Assert.If(!shopObjectTypes.Contains(ownerObjectType), "Object Type Error");

            Assert.If(!await ChatObjectManager.IsSomeRootAsync(sessionUnit.OwnerId, waiterId), "Is not some root");

            var shopWaiter = await ChatObjectManager.GetAsync(waiterId);

            var waiterSessionUnit = await SessionUnitManager.CreateIfNotContainsAsync(
                    session: sessionUnit.Session,
                    owner: shopWaiter,
                    destination: sessionUnit.Destination,
                    x =>
                    {
                        x.JoinWay = JoinWays.AutoJoin;
                    });

            if (sessionUnit.OwnerObjectType != ChatObjectTypeEnums.ShopKeeper)
            {
                sessionUnit.Setting.IsDisplay = false;
            }
            waiterSessionUnit.Setting.IsDisplay = true;
            waiterSessionUnit.Setting.IsInputEnabled = true;
            if (isNotice)
            {
                await MessageSender.SendCmdAsync(sessionUnit, new MessageInput<CmdContentInfo>()
                {
                    Content = new CmdContentInfo()
                    {
                        Cmd = MessageKeyNames.Transfer,
                        Text = new TextTemplate("{source} 转接给 {destination}")
                         .WithData("source", new SessionUnitTextTemplate(sessionUnit))
                         .WithData("destination", new SessionUnitTextTemplate(waiterSessionUnit))
                         .ToString(),
                    }
                });
            }
            return waiterSessionUnit;
        }
    }
}
