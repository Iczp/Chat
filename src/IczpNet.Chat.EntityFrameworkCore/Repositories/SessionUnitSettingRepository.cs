using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Timing;

namespace IczpNet.Chat.Repositories;

public class SessionUnitSettingRepository(
    IDbContextProvider<ChatDbContext> dbContextProvider,
    IClock clock) : ChatRepositoryBase<SessionUnitSetting>(dbContextProvider), ISessionUnitSettingRepository
{
    protected IClock Clock { get; } = clock;

    public async Task<int> UpdateLastSendMessageAsync(Guid senderSessionUnitId, long lastSendMessageId, DateTime lastSendTime)
    {
        var context = await GetDbContextAsync();
        return await context.SessionUnitSetting
            .Where(x => x.SessionUnitId == senderSessionUnitId)
            .Where(x => x.LastSendMessageId == null || x.LastSendMessageId.Value < lastSendMessageId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.LastModificationTime, b => Clock.Now)
                .SetProperty(b => b.LastSendMessageId, b => lastSendMessageId)
                .SetProperty(b => b.LastSendTime, b => lastSendTime)
            );
    }
}
