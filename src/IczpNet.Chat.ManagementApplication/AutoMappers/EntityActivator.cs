using System;

namespace IczpNet.Chat.AutoMappers;

internal static class EntityActivator
{
    public static T Create<T>() => (T)Activator.CreateInstance(typeof(T), nonPublic: true)!;
}
