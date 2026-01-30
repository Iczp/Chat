using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionSections.SessionUnitRoles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IczpNet.Chat.SessionSections.SessionRoles;

[Index(nameof(IsDefault), AllDescending = true)]
[Index(nameof(PermissionCount), AllDescending = true)]
public class SessionRole : BaseEntity<Guid>, IName, IIsDefault, ISessionId
{
    protected SessionRole() { }

    public SessionRole(Guid id, Guid sessionId, string name) : base(id)
    {
        Name = name;
        SessionId = sessionId;
    }

    public virtual Guid? SessionId { get; protected set; }

    [ForeignKey(nameof(SessionId))]
    public virtual Session Session { get; set; }

    [StringLength(20)]
    public virtual string Name { get; set; }

    [StringLength(500)]
    public virtual string Description { get; set; }

    public virtual bool IsDefault { get; set; }

    public virtual int PermissionCount { get; protected set; }

    public virtual List<SessionUnitRole> SessionUnitRoleList { get; protected set; }

    public virtual IList<SessionPermissionRoleGrant> GrantList { get; set; }

    public void SetPermissionGrant(Dictionary<string, PermissionGrantValue> permissionGrant)
    {
        GrantList?.Clear();
        GrantList = permissionGrant.Select(x => new SessionPermissionRoleGrant(x.Key, Id, x.Value.Value, x.Value.IsEnabled)).ToList();
        PermissionCount = permissionGrant.Count;
    }

    public Dictionary<string, SessionPermissionRoleGrant> PermissionGrant => GetPermissionGrant();

    protected Dictionary<string, SessionPermissionRoleGrant> GetPermissionGrant()
    {
        return GrantList?.ToDictionary(x => x.DefinitionId);
    }

    public virtual void SetSessionId(Guid sessionId)
    {
        SessionId = sessionId;
    }
}
