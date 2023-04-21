using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    [Serializable]
    public class SessionUnitCacheItem
    {
        public virtual Guid Id { get; set; }

        public virtual Guid? SessionId { get; set; }

        public virtual ChatObjectTypeEnums? DestinationObjectType { get; set; }

        public virtual long OwnerId { get; set; }

        public virtual long? DestinationId { get; set; }

        public virtual bool IsPublic { get; set; }

        public virtual ServiceStatus ServiceStatus { get; set; }

    }
}
