using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages;

public interface IIncremenetBadge
{
    /// <summary>
    /// 更新角标是否应该作为后台作业
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<bool> ShouldbeBackgroundJobAsync(Message message);
}
