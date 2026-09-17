using IczpNet.Chat.AiRuns;
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

    /// <summary>
    /// 执行 AI 任务，携带运行上下文和取消令牌
    /// </summary>
    Task<AiRunExecutionResult> ExecuteAsync(AiRunContext context, System.Threading.CancellationToken cancellationToken = default);
}

