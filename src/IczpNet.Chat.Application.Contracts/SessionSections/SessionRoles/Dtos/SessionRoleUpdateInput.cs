using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.SessionPermissions;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.SessionRoles.Dtos;

public class SessionRoleUpdateInput : BaseInput
{
    [StringLength(20)]
    public virtual string Name { get; set; }

    [StringLength(500)]
    public virtual string Description { get; set; }

    [DefaultValue(false)]
    public virtual bool IsDefault { get; set; }

    public virtual Dictionary<string, PermissionGrantValue> PermissionGrant { get; set; }
}
