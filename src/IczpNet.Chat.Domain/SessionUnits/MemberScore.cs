using System;

namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// ZSet 中 SessionUnit 的排序分值（可逆）
/// score = sorting * Multiplier + unixMilliseconds
/// </summary>
public readonly record struct MemberScore(double Value)
{
    /// <summary>
    /// 1e13
    /// - sorting：整数（0 / 1 / 2 ...）
    /// - unix ms：13 位
    /// - double 有效精度约 15 位，安全
    /// </summary>
    public const double Multiplier = 1e13;

    /// <summary>
    /// 构造 score
    /// </summary>
    public static MemberScore Create(bool isCreator, DateTime creationTime)
    {
        var sorting = isCreator ? 1d : 0d;
        var unixMs = new DateTimeOffset(creationTime).ToUnixTimeMilliseconds();

        return new MemberScore(sorting * Multiplier + unixMs);
    }

    /// <summary>
    /// 是否创建者（从 score 反解析）
    /// </summary>
    public bool IsCreator
        => Math.Truncate(Value / Multiplier) > 0;

    /// <summary>
    /// 创建时间（从 score 反解析）
    /// </summary>
    public DateTime CreationTime
    {
        get
        {
            var unixMs = (long)Math.Truncate(Value % Multiplier);
            return DateTimeOffset.FromUnixTimeMilliseconds(unixMs).UtcDateTime;
        }
    }

    /// <summary>
    /// 隐式转换，便于 Redis API 使用
    /// </summary>
    public static implicit operator double(MemberScore score)
        => score.Value;

    public override string ToString()
        => Value.ToString("R");
}
