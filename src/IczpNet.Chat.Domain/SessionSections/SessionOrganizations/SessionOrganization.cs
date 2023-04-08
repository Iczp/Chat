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
        protected SessionOrganization() { }

        public SessionOrganization(string name, Guid sessionId, long? parentId) : base(name, parentId)
        {
            Name = name;
            SessionId = sessionId;
        }

        public virtual Guid? SessionId { get; protected set; }

        [ForeignKey(nameof(SessionId))]
        public virtual Session Session { get; protected set; }

        public virtual List<SessionUnitOrganization> SessionUnitOrganizationList { get; protected set; }
    }
}
