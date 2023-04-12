using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.SessionPermissionGroups
{
    public class SessionPermissionGroup : BaseTreeEntity<SessionPermissionGroup, long>
    {
        protected SessionPermissionGroup() { }

        public SessionPermissionGroup(string name, long? parentId) : base(name, parentId) { }

        public virtual IList<SessionPermissionDefinition> DefinitionList { get; set; }
    }
}
