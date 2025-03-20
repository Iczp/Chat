using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.SessionPermissionGroups;

[Index(nameof(Sorting), AllDescending = true)]
[Index(nameof(FullPath), AllDescending = false)]
public class SessionPermissionGroup : BaseTreeEntity<SessionPermissionGroup, long>
{
    protected SessionPermissionGroup() { }

    public SessionPermissionGroup(string name, long? parentId) : base(name, parentId) { }

    public virtual IList<SessionPermissionDefinition> DefinitionList { get; set; }
}
