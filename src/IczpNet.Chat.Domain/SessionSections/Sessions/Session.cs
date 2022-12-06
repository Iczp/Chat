using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class Session : BaseEntity<Guid>
    {
        [StringLength(100)]
        public virtual string Value { get; set; }

        public virtual MessageChannels MessageChannel { get; set; }
    }
}
