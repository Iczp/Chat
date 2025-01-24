using IczpNet.AbpCommons;
using System;
using System.Reflection;

namespace IczpNet.Chat.Ai;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AiAttribute(string name) : Attribute
{
    /// <summary>
    /// Ai名称/聊天机器人编码
    /// </summary>
    public string Name { get; } = name;

    public static string GetName<T>()
    {
        return GetName(typeof(T));
    }

    public static string GetName(Type type)
    {
        var attribute = type.GetCustomAttribute<AiAttribute>();
        Assert.NotNull(attribute, $"Non-existent {nameof(AiAttribute)} attribute of type:'{type}'.");
        return attribute.Name;
    }
}