using IczpNet.Chat.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using System;
using IczpNet.Chat.HttpRequests;

namespace IczpNet.Chat.Repositories;

public class HttpRequestRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : ChatRepositoryBase<HttpRequest, Guid>(dbContextProvider), IHttpRequestRepository
{
}
