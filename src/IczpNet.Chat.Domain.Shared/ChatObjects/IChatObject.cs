using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.ChatObjects;

public interface IChatObject : IName, ICode
{
    long Id { get; }

    long? ParentId { get; }

    string Portrait { get; }

    string ChatObjectTypeId { get; }

    ChatObjectTypeEnums? ObjectType { get; }

    Guid? AppUserId { get; }
}
