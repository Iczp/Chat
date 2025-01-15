using IczpNet.Chat.ChatObjects;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Developers;


/// <summary>
/// 开发者管理
/// </summary>
public interface IDeveloperManager : IDomainService
{
    /// <summary>
    /// 是否启用开发者
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    Task<bool> IsEnabledAsync(ChatObject owner);

    /// <summary>
    /// 是否启用开发者
    /// </summary>
    /// <param name="receiverId"></param>
    /// <returns></returns>
    Task<bool> IsEnabledAsync(long? receiverId);
}
