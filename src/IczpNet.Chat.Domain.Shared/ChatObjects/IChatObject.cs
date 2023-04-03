using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.ChatObjects
{
    public interface IChatObject : IName, ICode
    {
        long Id { get; }

        string ChatObjectTypeId { get; }

        ChatObjectTypeEnums? ObjectType { get; }
    }
}
