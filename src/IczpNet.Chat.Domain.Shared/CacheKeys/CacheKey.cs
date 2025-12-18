using System;

namespace IczpNet.Chat.CacheKeys;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class CacheKey<T> : IEquatable<CacheKey<T>>
    where T : CacheKey<T>
{
    /// <summary>
    /// 子类必须返回一个稳定、唯一的 Hash 值来源
    /// </summary>
    protected abstract int GetKeyHashCode();

    /// <summary>
    /// 子类必须实现“值相等”
    /// </summary>
    protected abstract bool EqualsCore(T other);

    public sealed override bool Equals(object obj)
        => obj is T other && Equals(other);

    public bool Equals(CacheKey<T> other)
        => other is T typed && EqualsCore(typed);

    public sealed override int GetHashCode()
        => GetKeyHashCode();

    public static bool operator ==(CacheKey<T> left, CacheKey<T> right)
        => Equals(left, right);

    public static bool operator !=(CacheKey<T> left, CacheKey<T> right)
        => !Equals(left, right);
}