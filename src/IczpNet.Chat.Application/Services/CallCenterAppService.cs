using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.CallCenters;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace IczpNet.Chat.Services;

/// <summary>
/// 呼叫中心
/// </summary>
public class CallCenterAppService(ICallCenterManager callCenterManager) : ChatAppService, ICallCenterAppService
{
    protected ICallCenterManager CallCenterManager { get; } = callCenterManager;

    /// <summary>
    /// 转接
    /// </summary>
    /// <param name="sessionUnitId">当前会话单元Id</param>
    /// <param name="destinationId">目标会话单元Id</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<SessionUnitOwnerDetailDto> TransferToAsync([Required] Guid sessionUnitId, [Required] long destinationId)
    {
        var entity = await CallCenterManager.TransferToAsync(sessionUnitId, destinationId, isNotice: true);

        return ObjectMapper.Map<SessionUnit, SessionUnitOwnerDetailDto>(entity);
    }
}
