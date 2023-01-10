using System;
using System.Reflection;

namespace IczpNet.Chat.Attributes
{
    public class ContentOuputAttribute : Attribute
    {
        public Type OuputType { get; }

        public ContentOuputAttribute(Type outputType)
        {
            OuputType = outputType;
        }

        public static Type GetOuputType<T>()
        {
            return GetOuputType(typeof(T));
        }

        public static Type GetOuputType(Type type)
        {
            var nameAttribute = type.GetCustomAttribute<ContentOuputAttribute>();

            return nameAttribute.OuputType;
        }
    }
}
