using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.UrlNormalizers;

public class UrlNormalizer(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration) : IUrlNormalizer, ISingletonDependency
{
    public IHttpContextAccessor HttpContextAccessor { get; } = httpContextAccessor;
    protected virtual string SelfUrl { get; set; } = configuration["App:SelfUrl"]?.TrimEnd('/');

    protected virtual string BaseUrl => string.IsNullOrWhiteSpace(SelfUrl) ? RequestBaseUrl() : SelfUrl;

    public virtual string Normalize(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return url;
        }

        // 已经是绝对地址
        if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        if (string.IsNullOrEmpty(BaseUrl))
        {
            return url;
        }
        return $"{BaseUrl}{url}";
    }

    public virtual string ToRelative(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return url;
        }

        if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        var baseUrl = BaseUrl;
        if (!string.IsNullOrEmpty(baseUrl) &&
            url.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
        {
            var relative = url.Substring(baseUrl.Length);
            return relative.StartsWith('/') ? relative : $"/{relative}";
        }

        return url;
    }

    protected virtual string RequestBaseUrl()
    {
        var request = HttpContextAccessor.HttpContext?.Request;
        return request == null ? null : $"{request.Scheme}://{request.Host}";
    }
}
