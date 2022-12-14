using Microsoft.Extensions.Options;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
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

        public virtual async Task<Connection> OnlineAsync(Connection connection)
        {
            var entity = await Repository.InsertAsync(connection, autoSave: true);


            return entity;
        }
        public Task<int> GetOnlineCountAsync(DateTime currentTime)
        {
            return Repository.CountAsync(x => x.ActiveTime > currentTime.AddSeconds(-Config.InactiveSeconds));
        }

        public Task<Connection> GetAsync(Guid connectionId)
        {
            return Repository.GetAsync(connectionId);
        }

        public virtual async Task<Connection> UpdateActiveTimeAsync(Guid connectionId)
        {
            var entity = await Repository.GetAsync(connectionId);
            entity.SetActiveTime(Clock.Now);
            return await Repository.UpdateAsync(entity, true);
        }

        public virtual Task OfflineAsync(Guid connectionId)
        {
            return Repository.DeleteAsync(connectionId);
        }

        public virtual async Task<int> DeleteInactiveAsync()
        {
            Expression<Func<Connection, bool>> predicate = x => x.ActiveTime < Clock.Now.AddSeconds(-Config.InactiveSeconds);

            var count = await Repository.CountAsync(predicate);

            await Repository.DeleteAsync(predicate);

            return count;
        }
    }
}
