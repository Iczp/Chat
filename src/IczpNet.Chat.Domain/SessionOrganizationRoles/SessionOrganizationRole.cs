using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.SessionOrganizations;
using IczpNet.Chat.SessionRoles;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionOrganizationRoles;

/// <summary>
/// 会话组织与角色绑定关系（不向子组织继承）
/// </summary>
public class SessionOrganizationRole : BaseEntity
{
    public virtual long SessionOrganizationId { get; set; }

    [ForeignKey(nameof(SessionOrganizationId))]
    public virtual SessionOrganization SessionOrganization { get; set; }

    public virtual Guid SessionRoleId { get; set; }

    [ForeignKey(nameof(SessionRoleId))]
    public virtual SessionRole SessionRole { get; set; }

    protected SessionOrganizationRole() { }

    public SessionOrganizationRole(long sessionOrganizationId, Guid sessionRoleId)
    {
        SessionOrganizationId = sessionOrganizationId;
        SessionRoleId = sessionRoleId;
    }

    public SessionOrganizationRole(SessionOrganization sessionOrganization, SessionRole sessionRole)
    {
        SessionOrganization = sessionOrganization;
        SessionOrganizationId = sessionOrganization.Id;
        SessionRole = sessionRole;
        SessionRoleId = sessionRole.Id;
    }

    public override object[] GetKeys()
    {
        return new object[] { SessionOrganizationId, SessionRoleId };
    }
}
