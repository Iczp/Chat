using System.Threading.Tasks;
using System;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionUnitSettings;

public interface ISessionUnitSettingRepository : IRepository<SessionUnitSetting>
{
    Task<int> UpdateLastSendMessageAsync(Guid senderSessionUnitId, long lastSendMessageId, DateTime lastSendTime);
}
