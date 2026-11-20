using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Services;
using IczpNet.Chat.SessionUnits.Dtos;

namespace IczpNet.Chat.CallCenters
{
    public interface ICallCenterAppService : IApplicationService
    {
        Task<CallCenterConfigurationDto> GetConfigurationAsync(long shopKeeperId);
        Task<CallCenterConfigurationDto> UpdateConfigurationAsync(long shopKeeperId, UpdateCallCenterConfigurationInput input);
        Task DeleteConfigurationAsync(long shopKeeperId);
        Task<Dictionary<long, int>> GetActiveCountsAsync(long shopKeeperId);
        Task<CallCenterAssignmentDto> GetAssignmentAsync(Guid sessionId);
        Task<int> DrainQueueAsync(long shopKeeperId);
        Task CancelQueueAsync(Guid sessionId);

        /// <summary>
        /// 转接
        /// </summary>
        /// <param name="sessionUnitId">当前会话单元Id</param>
        /// <param name="destinationId">目标会话单元Id</param>
        /// <returns></returns>
        Task<SessionUnitOwnerDetailDto> TransferToAsync(Guid sessionUnitId, long destinationId);
    }
}
