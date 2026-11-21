using System;

namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// ZSet 中 Member 的排序分值（可逆）
/// 目标排序：群主 (IsCreator DESC, 第1梯队) -> 管理员 (IsAdmin DESC, 第2梯队) -> 普通成员 (第3梯队)，同级内部 JoinTime ASC (早入群在前)
/// 算法：score = (isCreator ? 3.0 : (isAdmin ? 2.0 : 1.0)) * Multiplier - unixMilliseconds
/// </summary>
public readonly record struct MemberScore(double Value)
{
    /// <summary>
    /// 基准乘数：1e13 (足够容纳 13 位毫秒时间戳)
    /// </summary>
    public const double Multiplier = 1e13;

    /// <summary>
    /// 构造分值（支持群主、管理员、普通成员三级排序）
    /// 逻辑：
    /// 群主范围：[3 * 1e13 - unixMs] -> 约 2.82e13
    /// 管理员范围：[2 * 1e13 - unixMs] -> 约 1.82e13
    /// 普通成员范围：[1 * 1e13 - unixMs] -> 约 0.82e13
    /// 这样 DESC 排序时：群主 > 管理员 > 普通成员；且同级内时间戳越小（越早入群），分值越大，排在越前面。
    /// </summary>
    public static MemberScore Create(bool isCreator, bool isAdmin, DateTime creationTime)
    {
        var unixMs = new DateTimeOffset(creationTime).ToUnixTimeMilliseconds();
        var baseScore = isCreator ? 3.0 : (isAdmin ? 2.0 : 1.0);
        return new MemberScore(baseScore * Multiplier - unixMs);
    }

    /// <summary>
    /// 兼容重载（默认为非管理员）
    /// </summary>
    public static MemberScore Create(bool isCreator, DateTime creationTime) => Create(isCreator, false, creationTime);

    /// <summary>
    /// 是否创建者/群主（分值 >= 2e13）
    /// </summary>
    public bool IsCreator => Value >= 2.0 * Multiplier;

    /// <summary>
    /// 是否管理员（1e13 &lt;= 分值 &lt; 2e13）
    /// </summary>
    public bool IsAdmin => Value >= Multiplier && Value < 2.0 * Multiplier;

    /// <summary>
    /// 入群时间（从 score 反解析）
    /// </summary>
    public DateTime CreationTime
    {
        get
        {
            var baseFactor = IsCreator ? 3.0 : (IsAdmin ? 2.0 : 1.0);
            var unixMs = (long)Math.Round(baseFactor * Multiplier - Value);
            return DateTimeOffset.FromUnixTimeMilliseconds(unixMs).UtcDateTime;
        }
    }

    public static implicit operator double(MemberScore score) => score.Value;

    public override string ToString() => Value.ToString("R");
}