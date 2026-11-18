using IczpNet.Chat.CacheKeys;
using System;

namespace IczpNet.Chat.QrCodes;

public class QrCodeCacheKey(string code) : CacheKey<QrCodeCacheKey>
{
    public string Code { get; set; } = code;

    public QrCodeCacheKey() : this(string.Empty)
    {
    }

    public bool Equals(QrCodeCacheKey other)
        => other != null && EqualsCore(other);

    protected override bool EqualsCore(QrCodeCacheKey other)
        => string.Equals(Code, other.Code, StringComparison.Ordinal);

    protected override int GetKeyHashCode()
        => Code != null ? StringComparer.Ordinal.GetHashCode(Code) : 0;

    public override string ToString()
        => $"{nameof(QrCodeCacheKey)}-{nameof(Code)}:{Code}";
}
