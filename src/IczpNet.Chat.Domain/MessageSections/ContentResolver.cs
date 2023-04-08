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
        public ConcurrentDictionary<string, Type> Providers => _providers;

        private readonly ConcurrentDictionary<string, Type> _providers = new();

        public ContentResolver()
        {
            var typeList = typeof(IContentProvider).Assembly.GetExportedTypes()
                    .Where(t => !t.IsAbstract && t.GetInterfaces().Any(x => typeof(IContentProvider).IsAssignableFrom(x)))
                    .ToList();

            foreach (var type in typeList)
            {
                _providers.TryAdd(type.GetCustomAttribute<ContentProviderAttribute>(true).ProviderName, type);
            }
        }

        public Type GetProviderType(string name)
        {
            return _providers[name];
        }

        public Type GetProviderTypeOrDefault(string name)
        {
            return _providers.GetOrDefault(name);
        }
    }
}
