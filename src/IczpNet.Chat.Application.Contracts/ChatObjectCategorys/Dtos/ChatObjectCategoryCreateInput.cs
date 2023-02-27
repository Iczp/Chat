namespace IczpNet.Chat.ChatObjectCategorys.Dtos;

/// <summary>
/// ChatObjectCategory CreateInput
/// </summary>
public class ChatObjectCategoryCreateInput : ChatObjectCategoryUpdateInput
{
    public virtual string ChatObjectTypeId { get; set; }
}
