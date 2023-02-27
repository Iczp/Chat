using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.ChatObjectCategorys.Dtos;

/// <summary>
/// ChatObjectCategory GetListInput
/// </summary>
public class ChatObjectCategoryGetListInput : BaseTreeGetListInput
{
    public virtual string ChatObjectTypeId { get; set; }
}
