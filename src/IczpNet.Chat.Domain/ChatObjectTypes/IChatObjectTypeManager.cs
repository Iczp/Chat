using IczpNet.Chat.Enums;
using System.Threading.Tasks;

namespace IczpNet.Chat.ChatObjectTypes
{
    public interface IChatObjectTypeManager
    {
        Task<ChatObjectType> GetAsync(string id);

        Task<ChatObjectType> GetAsync(ChatObjectTypeEnums chatObjectTypeEnum);
    }
}
