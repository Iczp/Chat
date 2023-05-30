using System;
using Volo.Abp.Domain.Repositories;


namespace IczpNet.Chat.HttpRequests
{
    /// <summary>
    /// IHttpRequestRepository 仓储接口
    /// </summary>
    public interface IHttpRequestRepository : IRepository<HttpRequest, Guid>
    {
    }
}
