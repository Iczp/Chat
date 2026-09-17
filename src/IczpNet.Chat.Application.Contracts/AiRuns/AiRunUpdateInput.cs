using System;

namespace IczpNet.Chat.AiRuns;

[Serializable]
public class AiRunUpdateInput
{
    /// <summary>
    /// 状态
    /// </summary>
    public virtual AiRunStatus? Status { get; set; }

    /// <summary>
    /// 下次尝试时间
    /// </summary>
    public virtual DateTime? NextAttemptAt { get; set; }

    /// <summary>
    /// 最大尝试次数
    /// </summary>
    public virtual int? MaxAttempts { get; set; }
}
