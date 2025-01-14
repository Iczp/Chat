using IczpNet.Chat.ChatObjects;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Developers;

public interface IDeveloperManager : IDomainService
{
    Task<bool> IsEnabledAsync(ChatObject owner);
}
