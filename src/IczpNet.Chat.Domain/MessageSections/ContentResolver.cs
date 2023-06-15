using IczpNet.Chat.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.MessageSections
{
    public class ContentResolver : IContentResolver, ISingletonDependency
    {
        public ConcurrentDictionary<MessageTypes, Type> Providers => _providers;

        private readonly ConcurrentDictionary<MessageTypes, Type> _providers = new();

        public ContentResolver()
        {
            var typeList = typeof(IContentProvider).Assembly.GetExportedTypes()
                    .Where(t => !t.IsAbstract && t.GetInterfaces().Any(x => typeof(IContentProvider).IsAssignableFrom(x)))
                    .ToList();

            foreach (var type in typeList)
            {
                _providers.TryAdd(type.GetCustomAttribute<ContentProviderAttribute>(true).MessageType, type);
            }
        }

        public Type GetProviderType(MessageTypes messageType)
        {
            return _providers[messageType];
        }

        public Type GetProviderTypeOrDefault(MessageTypes messageType)
        {
            return _providers.GetOrDefault(messageType);
        }
    }
}
