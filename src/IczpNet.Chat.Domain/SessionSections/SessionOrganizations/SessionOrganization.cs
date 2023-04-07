using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnitOrganizations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.SessionOrganizations
{
    public class SessionOrganization : BaseTreeEntity<SessionOrganization, long>
    {
        public SessionOrganization(string name, long? parentId) : base(name, parentId)
        {
            Name = name;
        }

        public virtual Guid? SessionId { get; set; }

        [ForeignKey(nameof(SessionId))]
        public virtual Session Session { get; set; }

        public virtual List<SessionUnitOrganization> SessionUnitOrganizationList { get; protected set; }
    }
}
