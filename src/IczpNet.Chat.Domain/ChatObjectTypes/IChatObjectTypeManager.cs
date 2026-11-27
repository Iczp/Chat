using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.ChatObjectTypes;

public class ChatObjectTypeAdminItemResult
{
    public ChatObjectType Entity { get; set; }

    public int ChatObjectCount { get; set; }
}

public class ChatObjectTypeAdminListResult
{
    public long TotalCount { get; set; }

    public List<ChatObjectTypeAdminItemResult> Items { get; set; } = new();
}

public interface IChatObjectTypeManager
{
    Task<ChatObjectType> GetAsync(string id);

    Task<ChatObjectType> GetAsync(ChatObjectTypeEnums chatObjectTypeEnum);

    Task<ChatObjectTypeAdminListResult> GetAdminListAsync(
        string keyword = null,
        int skipCount = 0,
        int maxResultCount = 50,
        string sorting = null,
        CancellationToken cancellationToken = default);

    Task<ChatObjectTypeAdminItemResult> GetAdminDetailAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<ChatObjectTypeAdminItemResult> CreateAdminAsync(
        string id,
        string name,
        ChatObjectTypeEnums? objectType,
        string description,
        int maxDepth,
        bool isHasChild,
        CancellationToken cancellationToken = default);

    Task<ChatObjectTypeAdminItemResult> UpdateAdminAsync(
        string id,
        string name,
        ChatObjectTypeEnums? objectType,
        string description,
        int maxDepth,
        bool isHasChild,
        CancellationToken cancellationToken = default);

    Task DeleteAdminAsync(
        string id,
        CancellationToken cancellationToken = default);
}
