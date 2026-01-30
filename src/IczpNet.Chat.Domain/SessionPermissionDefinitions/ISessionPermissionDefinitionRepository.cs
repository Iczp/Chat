using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionPermissionDefinitions;

public interface ISessionPermissionDefinitionRepository : IRepository<SessionPermissionDefinition, string>
{
    Task<int> BatchUpdateIsEnabledAsync(bool isEnabled);
}
