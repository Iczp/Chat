

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace IczpNet.Chat.Middlewares;
public class UrlToAuthorizationHeaderMiddleware(RequestDelegate next, string tokenParameterName = "access_token")
{
    private readonly RequestDelegate _next = next;
    private readonly string _tokenParameterName = tokenParameterName;  // 用于配置从哪个 URL 参数中获取 Token

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Query[_tokenParameterName].ToString();

        if (!string.IsNullOrEmpty(token))
        {
            // 将 Token 添加到 Authorization 请求头
            context.Request.Headers.Append("Authorization", $"Bearer {token}");
        }

        // 继续请求管道中的下一个中间件
        await _next(context);
    }
}