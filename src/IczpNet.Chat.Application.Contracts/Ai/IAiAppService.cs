using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Ai;

public interface IAiAppService : IApplicationService
{
    Task<AiRunStateDto> GetActiveAsync(Guid sessionUnitId);
    Task<List<AiRunStateDto>> GetRecentAsync(Guid sessionUnitId, int maxResultCount = 20);
}
