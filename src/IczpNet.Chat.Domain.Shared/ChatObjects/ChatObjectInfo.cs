﻿using IczpNet.AbpTrees;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.ChatObjects;

public class ChatObjectInfo : TreeInfo<long>, IChatObject
{
    public virtual string ChatObjectTypeId { get; set; }

    public virtual string Code { get; set; }

    public virtual string Description { get; set; }

    public virtual Genders Gender { get; set; }

    public virtual string Thumbnail { get; set; }

    public virtual string Portrait { get; set; }

    public virtual Guid? AppUserId { get; set; }

    public virtual bool IsPublic { get; protected set; }

    public virtual bool IsEnabled { get; set; }

    public virtual bool IsDefault { get; set; }

    public virtual ChatObjectTypeEnums? ObjectType { get; set; }

    public virtual ServiceStatus? ServiceStatus { get; set; }
}
