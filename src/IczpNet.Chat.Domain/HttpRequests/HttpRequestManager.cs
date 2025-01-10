using IczpNet.AbpCommons.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.Json;

namespace IczpNet.Chat.HttpRequests;

/// <summary>
/// HttpRequestManager
/// </summary>
public class HttpRequestManager : DomainService, IHttpRequestManager
{
    protected static readonly string DefaultUserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
    protected IHttpClientFactory HttpClientFactory { get; }
    protected IHttpRequestRepository Repository { get; }
    protected IJsonSerializer JsonSerializer { get; }

    public HttpRequestManager(
        IHttpRequestRepository httpLogRepository,
        IHttpClientFactory httpClientFactory,
        IJsonSerializer jsonSerializer)
    {
        Repository = httpLogRepository;
        HttpClientFactory = httpClientFactory;
        JsonSerializer = jsonSerializer;
    }

    public async Task<HttpRequest> GetAsync(string url, HttpContent httpContent = null)
    {
        return await RequestAsync(HttpMethod.Get, url, null);
    }

    public async Task<HttpRequest> PostAsync(string url, HttpContent httpContent = null)
    {
        return await RequestAsync(HttpMethod.Post, url, httpContent);
    }


    public async Task<HttpRequest> RequestAsync(HttpMethod method, string url, HttpContent httpContent = null, IDictionary<string, string> headers = null, string userAgent = null, string name = null)
    {
        var httpRequestMessage = new HttpRequestMessage(method, url)
        {
            Headers = {
                { HeaderNames.UserAgent,$"{userAgent} im.iczp.net" }
            },
        };

        if (headers != null)
        {
            foreach (var header in headers)
            {
                httpRequestMessage.Headers.Add(header.Key, header.Value);
            }
        }

        if (httpContent != null)
        {
            httpRequestMessage.Content = httpContent;
            //var parma= new StringContent(JsonSerializer.Serialize(httpContent), Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);
        }

        var uri = httpRequestMessage.RequestUri;

        var req = new HttpRequest
        {
            Name = name,
            IsSuccess = false,
            HttpMethod = method.ToString(),
            Timeout = 30,
            UserAgent = userAgent,
            Parameters = httpRequestMessage.Content?.ToString(),
            Headers = headers == null ? null : JsonSerializer.Serialize(httpRequestMessage.Headers),
            Url = url,
            Scheme = uri.Scheme,
            Host = uri.Host.MaxLength(500),
            Port = uri.Port,
            IsDefaultPort = uri.IsDefaultPort,
            Query = uri.Query.MaxLength(500),
            Fragment = uri.Fragment.MaxLength(500),
            AbsolutePath = uri.AbsolutePath.MaxLength(500),
            StartTime = Clock.Now.Ticks,
        };
        try
        {
            var stopwatch = Stopwatch.StartNew();

            var httpClient = HttpClientFactory.CreateClient(HttpRequest.ClientName);

            Logger.LogInformation($"http [{req.HttpMethod}]:{url}");

            using var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            req.Response = new HttpResponse()
            {
                Content = await httpResponseMessage.Content.ReadAsStringAsync()
            };

            req.ContentLength = req.Response.Content.Length;

            req.IsSuccess = httpResponseMessage.IsSuccessStatusCode;

            if (req.IsSuccess)
            {
                req.Message = "ok";
            }

            req.StatusCode = httpResponseMessage.StatusCode;

            req.Duration = stopwatch.ElapsedMilliseconds;

            Logger.LogInformation($"IsRequestSuccess {req.IsSuccess},stopwatch:${stopwatch.ElapsedMilliseconds}");
        }
        catch (Exception ex)
        {
            req.Message = ex.Message;

            req.IsSuccess = false;

            Logger.LogError(ex.Message);
        }

        req.EndTime = Clock.Now.Ticks;

        return await Repository.InsertAsync(req, autoSave: true);

    }
}