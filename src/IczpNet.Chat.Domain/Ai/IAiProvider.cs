using System.Threading.Tasks;

namespace IczpNet.Chat.Ai;

public interface IAiProvider
{
    /// <summary>
    /// Ai 名称
    /// </summary>
    string GetProviderName();

    /// <summary>
    /// Ai 模型
    /// </summary>
    string GetModel();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task HandleAsync(long messageId);
}

