using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;

namespace IczpNet.Chat.BaseDtos;



public class BaseTreeDto<TKey> : ExtensibleCreationAuditedEntityDto<TKey>, IHasCreationTime where TKey : struct
{
    public virtual TKey? ParentId { get; set; }

    public virtual string Name { get; set; }

    public virtual int ChildrenCount { get; set; }
}
