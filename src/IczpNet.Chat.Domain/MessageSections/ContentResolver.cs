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
            ContentProvider = typeof(IContentResolver).Assembly.GetExportedTypes()
                    .Where(t => !t.IsAbstract && t.GetInterfaces().Any(x => typeof(IContentResolver).IsAssignableFrom(x)))
                    .ToDictionary(x => x.GetCustomAttribute<ContentProviderAttribute>(true).ProviderName, x => x)
                    ;
        }

        public Type GetProvider(string name)
        {
            return ContentProvider[name];
        }
    }
}
