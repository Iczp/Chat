using IczpNet.Chat.BaseEntitys;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Connections
{
    public class Connection : BaseEntity<Guid>
    {
        //[Required]
        public virtual Guid? AppUserId { get; set; }

        public virtual Guid? ChatObjectId { get; set; }

        [StringLength(200)]
        public virtual string Server { get; set; }

        [StringLength(50)]
        public virtual string DeviceId { get; set; }

        [StringLength(36)]
        public virtual string Ip { get; set; }

        [StringLength(300)]
        public virtual string Agent { get; set; }

        public virtual DateTime ActiveTime { get; set; } = DateTime.Now;

        protected Connection() { }

    }
}
