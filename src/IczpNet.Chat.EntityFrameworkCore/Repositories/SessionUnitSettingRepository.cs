using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.Repositories;

public class SessionUnitSettingRepository : ChatRepositoryBase<SessionUnitSetting>, ISessionUnitSettingRepository
{
    public SessionUnitSettingRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public async Task<int> UpdateLastSendMessageAsync(Guid senderSessionUnitId, long lastSendMessageId, DateTime lastSendTime)
    {
        var context = await GetDbContextAsync();
        return await context.SessionUnitSetting
            .Where(x => x.SessionUnitId == senderSessionUnitId)
            .Where(x => x.LastSendMessageId == null || x.LastSendMessageId.Value < lastSendMessageId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.LastModificationTime, b => DateTime.Now)
                .SetProperty(b => b.LastSendMessageId, b => lastSendMessageId)
                .SetProperty(b => b.LastSendTime, b => lastSendTime)
            );
    }
}
