﻿using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Management.SessionSections.SessionPermissionDefinitions.Dtos;

public class SessionPermissionDefinitionSimpleDto : EntityDto<Guid>
{
    public virtual string Name { get; set; }
}
