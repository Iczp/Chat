using IczpNet.Chat.SessionSections.SessionPermissionDefinitions.Dtos;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.SessionPermissions.Dtos;

public class SessionPermissionGrantDto
{
    public SessionPermissionDefinitionDto Definition { get; set; }
    public List<SessionPermissionRoleGrantDto> RoleGrants { get; set; }
    public List<SessionPermissionUnitGrantDto> UnitGrants { get; set; }
}
