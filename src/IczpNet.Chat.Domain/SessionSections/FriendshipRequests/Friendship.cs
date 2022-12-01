using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.Friendships
{
    public class FriendshipRequest : BaseEntity<Guid>, IChatOwner<Guid>
    {
        public virtual Guid OwnerId { get; protected set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; protected set; }

        public virtual Guid? DestinationId { get; protected set; }

        [ForeignKey(nameof(DestinationId))]
        public virtual ChatObject Destination { get; protected set; }

        [StringLength(200)]
        public virtual string Message { get; protected set; }

        public virtual bool? IsAgreed { get; protected set; }

        [StringLength(200)]
        public virtual string HandlMessage { get; protected set; }

        [StringLength(200)]
        public virtual DateTime? HandlTime { get; protected set; }

        protected FriendshipRequest() { }

        public FriendshipRequest(ChatObject owner, ChatObject friend, string message)
        {
            Owner = owner;
            Destination = friend;
            Message = message;
        }

        public virtual void HandlRequest(bool isAgreed, string handlMessage)
        {
            IsAgreed = isAgreed;
            HandlMessage = handlMessage;
            HandlTime = DateTime.Now;
        }
    }
}
