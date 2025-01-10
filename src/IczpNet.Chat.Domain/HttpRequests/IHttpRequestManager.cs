using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace IczpNet.Chat.HttpRequests;

/// <summary>
/// IHttpRequestManager
/// </summary>
public interface IHttpRequestManager
{
    /// <summary>
    /// HttpGet
    /// </summary>
    /// <param name="url"></param>
    /// <param name="httpContent"></param>
    /// <returns></returns>
    Task<HttpRequest> GetAsync(string url, HttpContent httpContent = null);

    /// <summary>
    /// HttpPost
    /// </summary>
    /// <param name="url"></param>
    /// <param name="httpContent"></param>
    /// <returns></returns>
    Task<HttpRequest> PostAsync(string url, HttpContent httpContent = null);

    /// <summary>
    /// Request
    /// </summary>
    /// <param name="method"></param>
    /// <param name="url"></param>
    /// <param name="httpContent"></param>
    /// <param name="headers"></param>
    /// <param name="userAgent"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<HttpRequest> RequestAsync(HttpMethod method, string url, HttpContent httpContent = null, IDictionary<string, string> headers = null, string userAgent = null, string name = null);
}