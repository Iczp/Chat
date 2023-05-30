using IczpNet.Chat.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using System;
using IczpNet.Chat.HttpRequests;

namespace IczpNet.Chat.Repositories
{
    public class HttpRequestRepository : ChatRepositoryBase<HttpRequest, Guid>, IHttpRequestRepository
    {
        public HttpRequestRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }
    }
}
