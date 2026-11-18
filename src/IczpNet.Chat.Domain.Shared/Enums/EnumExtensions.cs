using System;
using System.ComponentModel;
using System.Reflection;

namespace IczpNet.Chat.Enums;

public static class EnumExtensions
{
    public static string GetEnumDescription(this Enum value)
    {
        if(value == null)
        {
            return null;
        }
        var field = value.GetType().GetField(value.ToString());

        var attr = field?.GetCustomAttribute<DescriptionAttribute>();

        return attr?.Description ?? value.ToString();
    }
}