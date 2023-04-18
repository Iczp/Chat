using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.SessionRequests
{
    public class SessionRequest : BaseSessionEntity<Guid>, IDeviceId, IIsEnabled
    {
        [StringLength(36)]
        public virtual string DeviceId { get; set; }

        [StringLength(200)]
        public virtual string RequestMessage { get; set; }

        public virtual bool IsHandled { get; set; }

        public virtual bool? IsAgreed { get; protected set; }

        [StringLength(200)]
        public virtual string HandleMessage { get; protected set; }

        [StringLength(200)]
        public virtual DateTime? HandleTime { get; protected set; }

        public virtual bool IsEnabled { get; protected set; } = true;

        public virtual Guid? HandlerId { get; set; }

        [ForeignKey(nameof(HandlerId))]
        public virtual SessionUnit Handler { get; set; }

        public virtual bool IsExpired { get; set; } = false;

        public virtual DateTime? ExpirationTime { get; protected set; }

        protected SessionRequest() { }

        public SessionRequest(Guid id, ChatObject owner, ChatObject destination, string message) : base(id)
        {
            Owner = owner;
            Destination = destination;
            RequestMessage = message;
            IsHandled = false;
        }

        public SessionRequest(long ownerId, long destinationId, string message)
        {
            OwnerId = ownerId;
            DestinationId = destinationId;
            RequestMessage = message;
        }

        private void HandleRequest(bool isAgreed, string handlMessage, Guid? handleSessionUnitId)
        {
            IsAgreed = isAgreed;
            IsHandled = true;
            HandleMessage = handlMessage;
            HandlerId = handleSessionUnitId;
            HandleTime = DateTime.Now;
        }

        public virtual void DisagreeRequest(string handlMessage, Guid? handleSessionUnitId)
        {
            HandleRequest(false, handlMessage, handleSessionUnitId);
        }

        public virtual void AgreeRequest(string handlMessage, Guid? handleSessionUnitId)
        {
            HandleRequest(true, handlMessage, handleSessionUnitId);
        }

        internal void SetIsEnabled(bool v)
        {
            IsEnabled = v;
        }

        public virtual bool IsExpiredTime()
        {
            return !ExpirationTime.HasValue || ExpirationTime.Value < DateTime.Now;
        }

        internal void SetExpirationTime(int hours)
        {
            ExpirationTime = DateTime.Now.AddHours(hours);
        }
    }
}
