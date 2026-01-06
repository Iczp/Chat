using System;

namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// ZSet 中 SessionUnit 的排序分值（可逆）
/// score = Sorting * Multiplier + publicBadge
/// </summary>
public readonly record struct PinnedScore(double Value)
{
    /// <summary>
    /// 1e13
    /// 说明：
    ///  - sorting 为整数（如类型、优先级）
    ///  - badge 使用毫秒（13 位）
    ///  - double 有效精度 ~15 位，留 2 位安全空间
    /// </summary>
    public const double Multiplier = 1e13;

    /// <summary>
    /// 构造 score
    /// </summary>
    public static PinnedScore Create(double sorting, double badge)
    {
        return new PinnedScore(sorting * Multiplier + badge);
    }

    /// <summary>
    /// 从 score 反解析排序值
    /// </summary>
    public double Sorting
        => (double)Math.Floor(Value / Multiplier);

    /// <summary>
    /// 从 score 反解析时间戳（毫秒）
    /// </summary>
    public double Badge
        => (double)(Value % Multiplier);

    /// <summary>
    /// 隐式转换，便于 Redis API 使用
    /// </summary>
    public static implicit operator double(PinnedScore score)
        => score.Value;

    public override string ToString()
        => Value.ToString("R");
}


