using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.AiRuns;

public interface IAiRunAppService : ICrudChatAppService<AiRunDetailDto, AiRunDto, Guid, AiRunGetListInput, AiRunCreateInput, AiRunUpdateInput>
{
    /// <summary>
    /// 根据源消息 Id 获取 AI 执行记录
    /// </summary>
    Task<AiRunDetailDto> GetBySourceMessageIdAsync(long sourceMessageId);

    /// <summary>
    /// 获取指定会话的 AI 执行记录列表
    /// </summary>
    Task<PagedResultDto<AiRunDto>> GetListBySessionAsync(Guid sessionId, AiRunGetListInput input);

    /// <summary>
    /// 人工重试执行失败或超时的任务
    /// </summary>
    Task<AiRunDetailDto> RetryAsync(Guid id);

    /// <summary>
    /// 取消任务
    /// </summary>
    Task<AiRunDetailDto> CancelAsync(Guid id, string reason = null);
}
