using IczpNet.Chat.BaseEntities;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Connections
{
    public class Connection : BaseEntity<Guid>
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public virtual long AutoId { get; protected set; }
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

        public virtual DateTime ActiveTime { get; private set; } = DateTime.Now;

        protected Connection() { }

        internal void SetActiveTime(DateTime now)
        {
            ActiveTime = now;
        }
    }
}
