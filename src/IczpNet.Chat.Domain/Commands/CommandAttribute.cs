using System;
using System.Reflection;

namespace IczpNet.Chat.Commands
{
    public class CommandAttribute : Attribute
    {
        public string Command { get; }

        public CommandAttribute(string command)
        {
            Command = command;
        }

        public static string GetValue<T>()
        {
            return GetValue(typeof(T));
        }

        public static string GetValue(Type type)
        {
            var nameAttribute = type.GetCustomAttribute<CommandAttribute>();

            if (nameAttribute == null)
            {
                return type.FullName;
            }
            return nameAttribute.Command;
        }
    }
}
