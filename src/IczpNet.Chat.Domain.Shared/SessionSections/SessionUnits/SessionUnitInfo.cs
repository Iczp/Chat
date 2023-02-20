using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    [Serializable]
    public class SessionUnitInfo
    {
        public virtual Guid Id { get; set; }

        public virtual Guid SessionId { get; set; }

        public virtual ChatObjectTypes? DestinationObjectType { get; set; }

        public virtual Guid OwnerId { get; set; }

        public virtual Guid? DestinationId { get; set; }
        
    }
}
