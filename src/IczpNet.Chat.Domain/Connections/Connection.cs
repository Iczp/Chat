﻿using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Connections
{
    public class Connection
    {
        public virtual Guid? OwnerUserId { get; set; }

        [StringLength(200)]
        public virtual string Server { get; set; }

        [StringLength(50)]
        public virtual string DeviceId { get; set; }

        [StringLength(36)]
        public virtual string Ip { get; set; }

        [StringLength(300)]
        public virtual string Agent { get; set; }

        public virtual DateTime ActiveTime { get; set; }
    }
}