using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.AiRuns;

[Serializable]
public class AiRunCreateInput
{
    /// <summary>
    /// 源消息 Id
    /// </summary>
    [Required]
    public virtual long SourceMessageId { get; set; }

    /// <summary>
    /// 会话 Id
    /// </summary>
    [Required]
    public virtual Guid SessionId { get; set; }

    /// <summary>
    /// 请求发起方 SessionUnitId
    /// </summary>
    [Required]
    public virtual Guid RequesterSessionUnitId { get; set; }

    /// <summary>
    /// 提供商（如 Aurora）
    /// </summary>
    [Required]
    [MaxLength(64)]
    public virtual string Provider { get; set; }

    /// <summary>
    /// 最大尝试次数
    /// </summary>
    public virtual int? MaxAttempts { get; set; }
}
