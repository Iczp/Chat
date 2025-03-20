using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionUnits;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.SessionPermissionUnitGrants;

public class SessionPermissionUnitGrant : BaseEntity
{
    protected SessionPermissionUnitGrant() { }

    public SessionPermissionUnitGrant(string definitionId, Guid sessionUnitId, long value, bool isEnabled)
    {
        DefinitionId = definitionId;
        SessionUnitId = sessionUnitId;
        Value = value;
        IsEnabled = isEnabled;
    }

    public virtual string DefinitionId { get; set; }

    [ForeignKey(nameof(DefinitionId))]
    public virtual SessionPermissionDefinition Definition { get; set; }

    public virtual Guid SessionUnitId { get; set; }

    [ForeignKey(nameof(SessionUnitId))]
    public virtual SessionUnit SessionUnit { get; set; }

    public virtual long Value { get; set; }

    public virtual bool IsEnabled { get; set; }

    public override object[] GetKeys()
    {
        return new object[] { DefinitionId, SessionUnitId, };
    }
}
