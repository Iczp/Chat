using System;

namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// ZSet 中 SessionUnit 的排序分值（可逆）
/// score = Sorting * Multiplier + Ticks
/// </summary>
public readonly record struct SessionUnitScore(double Value)
{
    /// <summary>
    /// 1e13
    /// 说明：
    ///  - sorting 为整数（如类型、优先级）
    ///  - ticks 使用毫秒（13 位）
    ///  - double 有效精度 ~15 位，留 2 位安全空间
    /// </summary>
    public const double Multiplier = 1e13;

    /// <summary>
    /// 构造 score
    /// </summary>
    public static SessionUnitScore Create(double sorting, double ticks)
    {
        return new SessionUnitScore(sorting * Multiplier + ticks);
    }

    /// <summary>
    /// 从 score 反解析排序值
    /// </summary>
    public double Sorting
        => (double)Math.Floor(Value / Multiplier);

    /// <summary>
    /// 从 score 反解析时间戳（毫秒）
    /// </summary>
    public double Ticks
        => (double)(Value % Multiplier);

    /// <summary>
    /// 隐式转换，便于 Redis API 使用
    /// </summary>
    public static implicit operator double(SessionUnitScore score)
        => score.Value;

    public override string ToString()
        => Value.ToString("R");
}


