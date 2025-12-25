using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using Volo.Abp;
namespace IczpNet.Chat.Options;

/// <summary>
/// 自动绑定所有实现 IConventionOptions 的 Options
/// 默认 Section = 类名去掉 Options 后缀
/// 可通过[OptionsSection("xxx")] 覆盖 Section 名称
/// 启动期错误（非 Options 后缀且无 Attribute）直接抛异常
/// </summary>
public static class ServiceCollectionOptionsExtensions
{
    public static IServiceCollection ConfigureByConvention<TOptions>(
        this IServiceCollection services,
        string sectionKey = null
    )
        where TOptions : class, IConventionOptions
    {
        string _sectionKey = sectionKey;

        if (string.IsNullOrWhiteSpace(_sectionKey))
        {
            var optionsType = typeof(TOptions);

            // 优先读取 Attribute
            var attr = optionsType.GetCustomAttribute<OptionsSectionAttribute>();
            if (attr != null)
            {
                _sectionKey = attr.SectionKey;
            }
            else
            {
                var optionsName = optionsType.Name;
                if (!optionsName.EndsWith("Options", StringComparison.Ordinal))
                {
                    throw new AbpException(
                        $"Type '{optionsType.FullName}' must end with 'Options' " +
                        $"or be decorated with [OptionsSection] to be configured by convention."
                    );
                }
                _sectionKey = optionsName.RemovePostFix("Options");
            }
        }

        var configuration = services.GetConfiguration();
        services.Configure<TOptions>(configuration.GetSection(_sectionKey));

        return services;
    }

    // 扫描程序集自动注册
    public static IServiceCollection ConfigureConventionModuleOptions(this IServiceCollection services, Type moduleType)
    {
        var assembly = moduleType.Assembly;
        var optionTypes = assembly.GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                typeof(IConventionOptions).IsAssignableFrom(t)
            );

        foreach (var type in optionTypes)
        {
            var method = typeof(ServiceCollectionOptionsExtensions)
                .GetMethod(nameof(ConfigureByConvention))!
                .MakeGenericMethod(type);

            method.Invoke(null, [services, null]);
        }

        return services;
    }

    public static IServiceCollection ConfigureConventionModuleOptions<TModule>(this IServiceCollection services)
    {
        return ConfigureConventionModuleOptions(services, typeof(TModule));
    }

    /// <summary>
    /// 扫描所有已加载的程序集，自动注册实现 IConventionOptions 的 Options。
    /// </summary>
    public static IServiceCollection ConfigureAllConventionOptions(this IServiceCollection services)
    {
        // 获取当前 AppDomain 所有已加载程序集
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            RegisterConventionOptionsFromAssembly(services, assembly);
        }

        return services;
    }

    /// <summary>
    /// 从指定程序集注册实现 IConventionOptions 的 Options
    /// </summary>
    private static void RegisterConventionOptionsFromAssembly(IServiceCollection services, Assembly assembly)
    {
        var optionTypes = assembly.GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                typeof(IConventionOptions).IsAssignableFrom(t)
            );

        foreach (var type in optionTypes)
        {
            var method = typeof(ServiceCollectionOptionsExtensions)
                .GetMethod(nameof(ConfigureByConvention), BindingFlags.Public | BindingFlags.Static)!
                .MakeGenericMethod(type);

            method.Invoke(null, [services, null]);
        }
    }
}
