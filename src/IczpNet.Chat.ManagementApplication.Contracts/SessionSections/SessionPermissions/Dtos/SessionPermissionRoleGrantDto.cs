using IczpNet.Chat.Management.SessionSections.SessionRoles.Dtos;

namespace IczpNet.Chat.Management.SessionSections.SessionPermissions.Dtos
{
    public class SessionPermissionRoleGrantDto
    {
        public virtual string DefinitionId { get; set; }

        public virtual long Value { get; set; }

        public virtual bool IsEnabled { get; set; }

        public virtual SessionRoleSimpleDto Role { get; set; }
    }
}
