using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class Session : BaseEntity<Guid>, IChatOwner<Guid?>
    {
        public virtual int MemberCount => GetMemberCount();

        [StringLength(80)]
        public virtual string SessionKey { get; protected set; }

        //[StringLength(36)]
        public virtual Channels Channel { get; protected set; }

        [StringLength(50)]
        public virtual string Title { get; set; }

        [StringLength(100)]
        public virtual string Description { get; set; }

        public virtual Guid? OwnerId { get; protected set; }

        public virtual ChatObject Owner { get; protected set; }

        public virtual List<Message> MessageList { get; internal set; } = new List<Message>();

        public virtual IList<SessionUnit> UnitList { get; internal set; } = new List<SessionUnit>();

        public virtual IList<Room> RoomList { get; protected set; } = new List<Room>();

        protected Session() { }

        public Session(Guid id, string sessionKey, Channels channel) : base(id)
        {
            SessionKey = sessionKey;
            Channel = channel;
        }

        internal virtual int GetMemberCount()
        {
            return UnitList.Count;
        }

        internal virtual void SetSessionUnitList(List<SessionUnit> unitList)
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
        }
    }
}
