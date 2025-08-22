﻿using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitDetailDto : SessionUnitOwnerDto
{

    /// <summary>
    /// 
    /// </summary>
    public virtual ChatObjectDto Owner { get; set; }

    /// <summary>
    /// 传话单元数量
    /// </summary>
    public virtual double SessionUnitCount { get; set; }

}
