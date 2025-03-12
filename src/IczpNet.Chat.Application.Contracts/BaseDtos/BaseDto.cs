using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;

namespace IczpNet.Chat.BaseDtos;

public class BaseDto : EntityDto
{

}

public class BaseDto<TKey> : ExtensibleCreationAuditedEntityDto<TKey>, IHasCreationTime
{

}


