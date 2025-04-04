﻿using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.ChatObjectTypes.Dtos;

public class ChatObjectTypeDto : BaseDto<string>
{
    public virtual string Name { get; set; }

    public virtual string Description { get; set; }

    public virtual int MaxDepth { get; set; }

    public virtual bool IsHasChild { get; set; }

    public int ChatObjectCount { get; set; }
}
