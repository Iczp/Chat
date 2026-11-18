using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IczpNet.Chat.Commands;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class CommandAttribute(string command) : Attribute
{
    public string Command { get; } = command;
    public static List<string> GetValues(Type type)
    {
        var attributes = type.GetCustomAttributes<CommandAttribute>();
        return attributes.Any() ? attributes.Select(x => x.Command).ToList() : [type.FullName!];
    }
}
