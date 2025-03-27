﻿using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.SessionSections.SessionUnits;

public interface ISessionUnit
{

    Guid Id { get;}

    /// <summary>
    /// 会话Id
    /// </summary>
    Guid? SessionId { get;}

    //Guid? UserId { get;}

    long OwnerId { get;}

    /// <summary>
    /// OwnerType
    /// </summary>
    ChatObjectTypeEnums? OwnerObjectType { get;}

    long? DestinationId { get;}

    /// <summary>
    /// DestinationType
    /// </summary>
    ChatObjectTypeEnums? DestinationObjectType { get;}

    /// <summary>
    /// 是否固定成员
    /// </summary>
    bool IsStatic { get;}

    /// <summary>
    /// 是否公开
    /// </summary>
    bool IsPublic { get;}

    /// <summary>
    /// 是否可见
    /// </summary>
    bool IsVisible { get;}

    /// <summary>
    /// 是否可用
    /// </summary>
    bool IsEnabled { get;}
}
