using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionTagDtos.Dtos
{
    public class SessionTagDto : EntityDto<Guid>
    {
        public virtual string Name { get; set; }
    }
}
