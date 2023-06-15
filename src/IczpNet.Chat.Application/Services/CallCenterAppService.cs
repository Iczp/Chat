using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.CallCenters;
using IczpNet.Chat.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.Services
{
    public class CallCenterAppService : ChatAppService, ICallCenterAppService
    {

        public CallCenterAppService() { }
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
