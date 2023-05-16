using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.CallCenters;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.Services
{
    public class CallCenterAppService : ChatAppService, ICallCenterAppService
    {

        protected IChatObjectManager ChatObjectManager { get; }
        protected ISessionUnitManager SessionUnitManager { get; }

        public CallCenterAppService(IChatObjectManager chatObjectManager, ISessionUnitManager sessionUnitManager)
        {
            ChatObjectManager = chatObjectManager;
            SessionUnitManager = sessionUnitManager;
        }

        [HttpPost]
        public async Task TransferToAsync(Guid sessionUnitId, long destinationId)
        {
            var entity = await SessionUnitManager.GetAsync(sessionUnitId);

            if (entity.Owner.ObjectType == ChatObjectTypeEnums.ShopWaiter)
            {

            }
            throw new NotImplementedException();
        }
    }
}
