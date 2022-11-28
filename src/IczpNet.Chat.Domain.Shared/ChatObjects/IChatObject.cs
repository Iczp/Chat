using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.ChatObjects
{
    public interface IChatObject : IName, ICode
    {
        ChatObjectTypeEnum ChatObjectType { get; }
    }
}
