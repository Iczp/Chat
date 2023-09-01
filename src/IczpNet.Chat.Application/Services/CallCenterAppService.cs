using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.CallCenters;
using IczpNet.Chat.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace IczpNet.Chat.Services
{
    /// <summary>
    /// 呼叫中心
    /// </summary>
    public class CallCenterAppService : ChatAppService, ICallCenterAppService
    {

        public CallCenterAppService() { }

        /// <summary>
        /// 转接
        /// </summary>
        /// <param name="sessionUnitId">当前会话单元Id</param>
        /// <param name="destinationId">目标会话单元Id</param>
        /// <returns></returns>
        [HttpPost]
        public async Task TransferToAsync([Required] Guid sessionUnitId, [Required] long destinationId)
        {
            var entity = await SessionUnitManager.GetAsync(sessionUnitId);

            if (entity.Owner.ObjectType == ChatObjectTypeEnums.ShopWaiter)
            {

            }
            throw new NotImplementedException();
        }
    }
}
