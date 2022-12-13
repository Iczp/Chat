using Microsoft.Extensions.Options;
using System;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Connections
{
    public class ConnectionManager : DomainService, IConnectionManager
    {
        protected IRepository<Connection, Guid> Repository { get; }
        protected ConnectionOptions Config { get; }
        public ConnectionManager(
            IRepository<Connection, Guid> repository, 
            IOptions<ConnectionOptions> options)
        {
            Repository = repository;
            Config = options.Value;
        }


    }
}
