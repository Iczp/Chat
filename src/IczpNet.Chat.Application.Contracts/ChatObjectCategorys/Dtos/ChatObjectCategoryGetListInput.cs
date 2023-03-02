using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.ChatObjectCategorys.Dtos;

/// <summary>
/// ChatObjectCategory GetListInput
/// </summary>
public class ChatObjectCategoryGetListInput : BaseTreeGetListInput<Guid>
{
    public virtual string ChatObjectTypeId { get; set; }
}
