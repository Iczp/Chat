using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.MessageSections
{
    public class ContentResolver : IContentResolver, ISingletonDependency
    {
        protected Dictionary<string, Type> ContentProvider { get; }
        public ContentResolver()
        {
            //ContentProvider = typeof(IContentProvider).Assembly.GetExportedTypes()
            //        .Where(t => !t.IsAbstract && t.GetInterfaces().Any(x => typeof(IContentProvider).IsAssignableFrom(x)))
            //        .ToDictionary(x => x.GetCustomAttribute<ContentProviderAttribute>(true).ProviderName, x => x)
            //        ;

            var list = typeof(IContentProvider).Assembly.GetExportedTypes()
                    .Where(t => !t.IsAbstract && t.GetInterfaces().Any(x => typeof(IContentProvider).IsAssignableFrom(x)))
                    .ToList();
            ContentProvider = list
                     .ToDictionary(x => x.GetCustomAttribute<ContentProviderAttribute>(true).ProviderName, x => x)
                    ;
        }

        public Type GetProviderType(string name)
        {
            return ContentProvider[name];
        }

        public Type GetProviderTypeOrDefault(string name)
        {
            return ContentProvider.GetOrDefault(name);
        }
    }
}
