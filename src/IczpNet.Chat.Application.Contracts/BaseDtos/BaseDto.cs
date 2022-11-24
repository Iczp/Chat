using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.BaseDtos
{
    public class BaseDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
    }
}
