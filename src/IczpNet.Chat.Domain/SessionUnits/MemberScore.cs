using System;

namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// ZSet 中 Member 的排序分值（可逆）
/// 目标排序：IsCreator DESC, JoinTime ASC
/// 算法：score = (isCreator ? 2.0 : 1.0) * Multiplier - unixMilliseconds
/// </summary>
public readonly record struct MemberScore(double Value)
{
    /// <summary>
    /// 基准乘数：1e13 (足够容纳 13 位毫秒时间戳)
    /// </summary>
    public const double Multiplier = 1e13;

    /// <summary>
    /// 构造分值
    /// 逻辑：
    /// 创建者范围：[2 * 1e13 - unixMs] -> 结果在 1.83e13 左右（假设当前时间）
    /// 普通成员范围：[1 * 1e13 - unixMs] -> 结果在 0.83e13 左右
    /// 这样 DESC 排序时：创建者永远大于普通成员；且时间戳越小（越早），分值越大，排在越前面。
    /// </summary>
    public static MemberScore Create(bool isCreator, DateTime creationTime)
    {
        var unixMs = new DateTimeOffset(creationTime).ToUnixTimeMilliseconds();
        // 使用 2.0 和 1.0 区分身份，并减去时间戳实现时间正序（分值倒序）
        var baseScore = isCreator ? 2.0 : 1.0;
        return new MemberScore(baseScore * Multiplier - unixMs);
    }

    /// <summary>
    /// 是否创建者（从 score 反解析）
    /// 只要分值大于等于 Multiplier (1e13)，说明基数是 2.0 减去了一个 13 位的时间戳
    /// 或者更简单的判断：Value >= Multiplier
    /// </summary>
    public bool IsCreator => Value >= Multiplier;

    /// <summary>
    /// 创建时间（从 score 反解析）
    /// </summary>
    public DateTime CreationTime
    {
        get
        {
            // 如果是创建者，原始计算是 2e13 - unixMs => unixMs = 2e13 - Value
            // 如果是普通人，原始计算是 1e13 - unixMs => unixMs = 1e13 - Value
            var baseFactor = IsCreator ? 2.0 : 1.0;
            var unixMs = (long)Math.Round(baseFactor * Multiplier - Value);
            return DateTimeOffset.FromUnixTimeMilliseconds(unixMs).UtcDateTime;
        }
    }

    public static implicit operator double(MemberScore score) => score.Value;

    public override string ToString() => Value.ToString("R");
}