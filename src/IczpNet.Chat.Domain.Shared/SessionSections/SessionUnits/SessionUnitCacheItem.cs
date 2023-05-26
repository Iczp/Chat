using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    [Serializable]
    public class SessionUnitCacheItem
    {
        public virtual Guid Id { get; set; }

        public virtual Guid? SessionId { get; set; }

        public virtual Guid? AppUserId { get; set; }

        //public virtual ChatObjectTypeEnums? DestinationObjectType { get; set; }

        public virtual long OwnerId { get; set; }

        public virtual long? DestinationId { get; set; }

        public virtual bool IsPublic { get; set; }

        public virtual long? ReadedMessageId { get; set; }

        public virtual long? LastMessageId { get; set; }

        public virtual int PublicBadge { get; set; }

        public virtual int PrivateBadge { get; set; }

        public virtual int RemindAllCount { get; set; }

        public virtual int RemindMeCount { get; set; }

        public virtual int FollowingCount { get; set; }

    }
}
