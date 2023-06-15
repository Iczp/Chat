using IczpNet.Chat.BaseEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Words
{
    [Index(nameof(Value), AllDescending = true)]
    public class Word : BaseEntity<Guid>
    {
        [MaxLength(36)]
        public virtual string Value { get; set; }

        public virtual bool IsEnabled { get; set; }

        public virtual bool IsDirty { get; set; }

        protected Word() { }
    }
}
