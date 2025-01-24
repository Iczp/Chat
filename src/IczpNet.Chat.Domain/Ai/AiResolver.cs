using Microsoft.Extensions.Logging;
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
    public ConcurrentDictionary<string, Type> Providers => _providers;

    private readonly ConcurrentDictionary<string, Type> _providers = new();

    public AiResolver()
    {
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
            var isAdd = _providers.TryAdd(typeName, type);
            //Logger.LogInformation($"Add AI provider '{typeName}' result:{isAdd}.");
        }
    }

    public Type GetProvider(string name)
    {
        return _providers[name];
    }

    public bool HasProvider(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && _providers.ContainsKey(name);
    }

    public Type GetProviderOrDefault(string name)
    {
        return _providers.GetOrDefault(name);
    }
}
