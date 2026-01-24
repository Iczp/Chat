using StackExchange.Redis;
using System;

namespace IczpNet.Chat.ConnectionPools;

public readonly record struct DeviceElement(
    string DeviceType,
    string DeviceId)
{
    public override string ToString()
        => $"{DeviceType}:{DeviceId}";

    /// <summary>
    /// 隐式转换，便于 Redis API 使用
    /// </summary>
    public static implicit operator RedisValue(DeviceElement element)
        => element.ToString();

    public static DeviceElement Create(string deviceType, string deviceId)
    {
        return new DeviceElement(deviceType, deviceId);
    }

    public static DeviceElement Parse(RedisValue element)
    {
        if (!TryParse(element, out var field))
        {
            throw new FormatException($"Invalid DeviceElement: {element}");
        }

        return field;
    }

    public static bool TryParse(RedisValue value, out DeviceElement field)
    {
        field = default;

        if (!value.HasValue)
        {
            return false;
        }

        var parts = value.ToString().Split(':');
        if (parts.Length != 2)
        {
            return false;
        }

        var deviceType = parts[0];
        var deviceId = parts[1];

        field = new DeviceElement(deviceType, deviceId);
        return true;
    }
}
