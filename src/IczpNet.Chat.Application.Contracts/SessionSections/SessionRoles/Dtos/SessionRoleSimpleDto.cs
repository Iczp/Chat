﻿using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionRoles.Dtos;

public class SessionRoleSimpleDto : EntityDto<Guid>
{
    public virtual string Name { get; set; }
}