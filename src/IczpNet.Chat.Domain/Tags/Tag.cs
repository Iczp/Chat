using IczpNet.Chat.BaseEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Tags
{
    [Index(nameof(Index))]
    [Index(nameof(Name))]
    public class Tag //: BaseEntity<Guid>
    {
        [MaxLength(20)]
        public virtual string Name { get; set; }

        [MaxLength(1)]
        public virtual string Index { get; set; }
    }
}
