using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionPermissions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections.SessionRoles
{
    public class SessionRoleManager : DomainService, ISessionRoleManager
    {
        protected IRepository<SessionRole, Guid> Repository { get; }
        protected ISessionPermissionDefinitionRepository SessionPermissionDefinitionRepository { get; }

        public SessionRoleManager(
            IRepository<SessionRole, Guid> repository, 
            ISessionPermissionDefinitionRepository sessionPermissionDefinitionRepository)
        {
            Repository = repository;
            SessionPermissionDefinitionRepository = sessionPermissionDefinitionRepository;
        }

        public async Task<SessionRole> SetAllPermissionsAsync(Guid id, PermissionGrantValue permissionGrantValue)
        {
            var entity = await Repository.GetAsync(id);

            var definitionList = await SessionPermissionDefinitionRepository.GetListAsync();

            var permissionGrant = definitionList.ToDictionary(x => x.Id, x => permissionGrantValue);

            entity.SetPermissionGrant(permissionGrant);

            await Repository.UpdateAsync(entity, autoSave: true);

            return entity;
        }
    }
}
