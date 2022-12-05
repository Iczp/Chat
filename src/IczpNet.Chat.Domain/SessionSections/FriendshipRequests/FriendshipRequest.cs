using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.SessionSections.Friendships;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace IczpNet.Chat.SessionSections.FriendshipRequests
{
    public class FriendshipRequest : BaseSessionEntity, IDeviceId
    {
        [StringLength(36)]
        public virtual string DeviceId { get; set; }

        [StringLength(200)]
        public virtual string Message { get; protected set; }

        public virtual bool IsHandled { get; set; }

        public virtual bool? IsAgreed { get; protected set; }

        [StringLength(200)]
        public virtual string HandlMessage { get; protected set; }

        [StringLength(200)]
        public virtual DateTime? HandlTime { get; protected set; }

        public virtual IList<Friendship> FriendshipList { get; protected set; }

        protected FriendshipRequest() { }

        public FriendshipRequest(ChatObject owner, ChatObject friend, string message)
        {
            Owner = owner;
            Destination = friend;
            Message = message;
            IsHandled = false;
        }

        public FriendshipRequest(Guid ownerId, Guid friendId, string message)
        {
            OwnerId = ownerId;
            DestinationId = friendId;
            Message = message;
        }

        private void HandlRequest(bool isAgreed, string handlMessage)
        {
            IsAgreed = isAgreed;
            IsHandled = true;
            HandlMessage = handlMessage;
            HandlTime = DateTime.Now;
        }

        public virtual void DisagreeRequest(string handlMessage)
        {
            HandlRequest(false, handlMessage);
        }
        public virtual void AgreeRequest(string handlMessage)
        {
            HandlRequest(true, handlMessage);
        }

        public List<Guid> GetFriendshipIdList()
        {
            return FriendshipList?.Select(d => d.Id).ToList();
        }
    }
}
