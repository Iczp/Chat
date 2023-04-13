using IczpNet.Chat.SessionSections.SessionRoles.Dtos;

namespace IczpNet.Chat.SessionSections.SessionPermissions.Dtos
{
    public class SessionPermissionRoleGrantDto
    {
        public virtual string DefinitionId { get; set; }

        public virtual long Value { get; set; }

        public virtual bool IsEnabled { get; set; }

        public virtual SessionRoleSimpleDto Role { get; set; }
    }
}
