using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionPermissionDefinitions.Dtos
{
    public class SessionPermissionDefinitionDto : EntityDto<string>
    {
        public virtual string Name { get; set; }
    }
}
