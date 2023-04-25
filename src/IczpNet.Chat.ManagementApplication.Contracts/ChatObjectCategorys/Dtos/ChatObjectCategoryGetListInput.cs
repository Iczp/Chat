using IczpNet.Chat.Management.BaseDtos;
using System;

namespace IczpNet.Chat.Management.ChatObjectCategorys.Dtos;

/// <summary>
/// ChatObjectCategory GetListInput
/// </summary>
public class ChatObjectCategoryGetListInput : BaseTreeGetListInput<Guid>
{
    public virtual string ChatObjectTypeId { get; set; }
}
