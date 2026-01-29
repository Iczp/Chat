using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Ai;

public class AiResolver : DomainService, IAiResolver, ISingletonDependency
{
    // 使用 Lazy<T> 确保 Providers 只初始化一次
    private static readonly Lazy<ConcurrentDictionary<string, Type>> lazyDictionary = new(GenerateDictionary);

    public static ConcurrentDictionary<string, Type> Providers => lazyDictionary.Value;

    public AiResolver()
    {

    }

    private static ConcurrentDictionary<string, Type> GenerateDictionary()
    {
        var dictionary = new ConcurrentDictionary<string, Type>();

        // 1. 获取所有已加载的程序集
        var typeList = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetExportedTypes())
            .Where(t => !t.IsAbstract && t.GetInterfaces().Any(x => typeof(IAiProvider).IsAssignableFrom(x)))
            .ToList();

        //var typeList = typeof(IAiProvider).Assembly.GetExportedTypes()
        //    .Where(t => !t.IsAbstract && t.GetInterfaces().Any(x => typeof(IAiProvider).IsAssignableFrom(x)))
        //    .ToList();

        //不要在构造器里输出日志，Logger未初始化
        //Logger.LogInformation($"AI provider count:{typeList.Count}.");

        foreach (var type in typeList)
        {
            var typeName = type.GetCustomAttribute<AiAttribute>(true)?.Name ?? type.FullName;
            var isAdd = dictionary.TryAdd(typeName, type);
            //Logger.LogInformation($"Add AI provider '{typeName}' result:{isAdd}.");
        }

        return dictionary;
    }

    public Type GetProvider(string name)
    {
        return Providers[name];
    }

    public bool HasProvider(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && Providers.ContainsKey(name);
    }

    public Type GetProviderOrDefault(string name)
    {
        return Providers.GetOrDefault(name);
    }
}
