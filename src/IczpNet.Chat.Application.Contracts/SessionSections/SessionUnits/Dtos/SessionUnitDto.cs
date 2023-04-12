using IczpNet.Chat.ChatObjects.Dtos;
using System;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitDto
    {
        public virtual Guid Id { get; set; }

        public virtual Guid SessionId { get; set; }

        public virtual long OwnerId { get; set; }

        /// <summary>
        /// 会话内的名称
        /// </summary>
        public virtual string MemberName { get; set; }

        /// <summary>
        /// 备注名称 Rename for destination
        /// </summary>
        public virtual string Rename { get; set; }

        public virtual long LastMessageId { get; set; }

        public virtual double Sorting { get; set; }

        public virtual ChatObjectDto Destination { get; set; }
    }
}
