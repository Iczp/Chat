using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Net.Http.Headers;
using Volo.Abp.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.HttpRequests
{
    /// <summary>
    /// HttpRequestManager
    /// </summary>
    public class HttpRequestManager : DomainService, IHttpRequestManager
    {
        protected static readonly string DefaultUserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
        protected IServiceScopeFactory ServiceScopeFactory { get; }
        protected IHttpClientFactory HttpClientFactory { get; }
        protected IHttpRequestRepository Repository { get; }
        protected IUnitOfWorkManager UnitOfWorkManager { get; }
        protected IJsonSerializer JsonSerializer { get; }

        public HttpRequestManager(IUnitOfWorkManager unitOfWorkManager,
            IHttpRequestRepository httpLogRepository,
            IHttpClientFactory httpClientFactory,
            IJsonSerializer jsonSerializer, IServiceScopeFactory serviceScopeFactory)
        {
            Repository = httpLogRepository;
            HttpClientFactory = httpClientFactory;
            UnitOfWorkManager = unitOfWorkManager;
            JsonSerializer = jsonSerializer;
            ServiceScopeFactory = serviceScopeFactory;
        }

        public async Task<HttpRequest> GetAsync(string url)
        {
            return await RequestAsync(HttpMethod.Get, url, null);
        }

        public async Task<HttpRequest> PostAsync(string url, HttpContent httpContent = null)
        {
            return await RequestAsync(HttpMethod.Post, url, httpContent);
        }
        

        public async Task<HttpRequest> RequestAsync(HttpMethod method, string url, HttpContent httpContent = null, IDictionary<string, string> headers = null, string userAgent = null)
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

            var req = new HttpRequest
            {
                IsSuccess = false,
                HttpMethod = HttpMethods.Get,
                Timeout = 30,
                UserAgent = userAgent,
                Parameters = httpRequestMessage.Content?.ToString(),
                Headers = headers == null ? null : JsonSerializer.Serialize(httpRequestMessage.Headers),
                Url = url,
                StartTime = Clock.Now.Ticks,
            };
            try
            {
                var httpClient = HttpClientFactory.CreateClient();

                Logger.LogInformation($"http [{req.HttpMethod}]:{url}");

                using var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

                req.ResponseContent = await httpResponseMessage.Content.ReadAsStringAsync();

                req.IsSuccess = httpResponseMessage.IsSuccessStatusCode;

                req.StatusCode = httpResponseMessage.StatusCode;

                Logger.LogInformation($"IsRequestSuccess {req.IsSuccess}");
            }
            catch (Exception ex)
            {
                req.ResponseContent = ex.Message;

                req.IsSuccess = false;

                Logger.LogError(ex.Message);
            }

            req.EndTime = Clock.Now.Ticks;

            return await Repository.InsertAsync(req, autoSave: true);

        }
    }
}