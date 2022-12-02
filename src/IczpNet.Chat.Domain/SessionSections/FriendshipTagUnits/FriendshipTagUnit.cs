using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.SessionSections.Friendships;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.FriendshipTagUnits
{
    public class FriendshipTagUnit : BaseEntity
    {
        public virtual Guid FriendshipId { get; protected set; }

        [ForeignKey(nameof(FriendshipId))]
        public virtual Friendship Friendship { get; protected set; }

        public virtual Guid FriendshipTagId { get; protected set; }

        [ForeignKey(nameof(FriendshipTagId))]
        public virtual FriendshipTag FriendshipTag { get; protected set; }

        protected FriendshipTagUnit() { }

        public FriendshipTagUnit(Friendship friendship,  FriendshipTag friendshipTag)
        {
            Friendship = friendship;
            FriendshipTag = friendshipTag;
        }

        public override object[] GetKeys()
        {
            return new object[] { FriendshipId, FriendshipTagId };
        }
    }
}
