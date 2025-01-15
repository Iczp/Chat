using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;

namespace IczpNet.Chat.BaseDtos;

public class BaseDto : EntityDto
{

}

public class BaseDto<TKey> : EntityDto<TKey>, IHasCreationTime
{
    public virtual DateTime CreationTime { get; set; }
}
