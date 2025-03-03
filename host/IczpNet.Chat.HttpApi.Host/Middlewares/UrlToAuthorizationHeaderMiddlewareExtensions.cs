
using Microsoft.AspNetCore.Builder;
namespace IczpNet.Chat.Middlewares;

public static class UrlToAuthorizationHeaderMiddlewareExtensions
{
    public static IApplicationBuilder UseUrlAuthorization(this IApplicationBuilder builder, string tokenParameterName = "access_token")
    {
        return builder.UseMiddleware<TokenToAuthorizationHeaderMiddleware>(tokenParameterName);
    }
}