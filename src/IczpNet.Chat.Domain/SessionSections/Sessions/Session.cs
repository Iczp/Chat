using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IczpNet.Chat.SessionSections.Sessions
{
    [Index(nameof(LastMessageId), AllDescending = true)]
    [Index(nameof(SessionKey))]
    public class Session : BaseEntity<Guid>, IChatOwner<long?>
    {
        [StringLength(80)]
        public virtual string SessionKey { get; protected set; }

        //[StringLength(36)]
        public virtual Channels Channel { get; protected set; }

        [StringLength(50)]
        public virtual string Title { get; set; }

        [StringLength(100)]
        public virtual string Description { get; set; }

        public virtual long? OwnerId { get; protected set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; protected set; }

        public virtual long? LastMessageId { get; protected set; }

        [ForeignKey(nameof(LastMessageId))]
        public virtual Message LastMessage { get; protected set; }

        public virtual IList<Message> MessageList { get; internal set; } = new List<Message>();

        public virtual IList<SessionUnit> UnitList { get; internal set; } = new List<SessionUnit>();

        public virtual IList<SessionTag> TagList { get; protected set; } = new List<SessionTag>();

        public virtual IList<SessionRole> RoleList { get; protected set; } = new List<SessionRole>();

        [NotMapped]
        public virtual int MemberCount => GetMemberCount();

        [NotMapped]
        public virtual int MessageCount => MessageList.Count();

        [NotMapped]
        public virtual int TagCount => TagList.Count();

        [NotMapped]
        public virtual int RoleCount => RoleList.Count();

        protected Session() { }

        public Session(Guid id, string sessionKey, Channels channel) : base(id)
        {
            SessionKey = sessionKey;
            Channel = channel;
        }

        internal virtual int GetMemberCount()
        {
            return UnitList.Count();
        }

        internal virtual void SetUnitList(List<SessionUnit> unitList)
        {
            UnitList = unitList;
        }

        public virtual void AddSessionUnit(SessionUnit sessionUnit)
        {
            UnitList.Add(sessionUnit);
        }

        internal void SetOwner(ChatObject chatObject)
        {
            Owner = chatObject;
            OwnerId = chatObject.Id;
        }

        internal void SetMessageList(List<Message> messages)
        {
            MessageList = messages;
        }

        internal SessionTag AddTag(SessionTag sessionTag)
        {
            TagList.Add(sessionTag);
            return sessionTag;
        }

        internal SessionRole AddRole(SessionRole sessionRole)
        {
            RoleList.Add(sessionRole);
            return sessionRole;
        }

        public void SetLastMessage(Message lastMessage)
        {
            LastMessage = lastMessage;
            LastMessageId = lastMessage.Id;
            LastMessageId = lastMessage.Id;
        }
    }
}
