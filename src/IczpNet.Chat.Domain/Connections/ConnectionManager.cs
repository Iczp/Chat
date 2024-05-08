using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Microsoft.Extensions.Logging;
using IczpNet.Chat.ServerHosts;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Connections;

public class ConnectionManager : DomainService, IConnectionManager
{
    protected IRepository<Connection, string> Repository { get; }
    protected IRepository<ServerHost, string> ServerHostRepository { get; }
    protected ConnectionOptions Config { get; }
    public ConnectionManager(
        IRepository<Connection, string> repository,
        IOptions<ConnectionOptions> options,
        IRepository<ServerHost, string> serverHostRepository)
    {
        Repository = repository;
        Config = options.Value;
        ServerHostRepository = serverHostRepository;
    }


    public virtual async Task<ConnectionOptions> GetConfigAsync()
    {
        await Task.Yield();
        return Config;
    }

    public virtual async Task<Connection> CreateAsync(Connection connection)
    {
        if (!connection.ServerHostId.IsNullOrWhiteSpace())
        {
            var serverHost = await ServerHostRepository.FindAsync(connection.ServerHostId);
            serverHost ??= await ServerHostRepository.InsertAsync(new ServerHost(connection.ServerHostId));
            connection.ServerHost = serverHost;
        }

        var entity = await Repository.InsertAsync(connection, autoSave: true);
        // 
        return entity;
    }
    public Task<int> GetOnlineCountAsync(DateTime currentTime)
    {
        return Repository.CountAsync(x => x.ActiveTime > currentTime.AddSeconds(-Config.InactiveSeconds));
    }

    public Task<Connection> GetAsync(string connectionId)
    {
        return Repository.GetAsync(connectionId);
    }

    [UnitOfWork]
    public virtual async Task<Connection> UpdateActiveTimeAsync(string connectionId)
    {
        //using var uow = UnitOfWorkManager.Begin();

        var entity = await Repository.FindAsync(connectionId);

        if (entity == null)
        {
            Logger.LogWarning($"No such connectionId:{connectionId}");
            return entity;
        }
        entity.SetActiveTime(Clock.Now);

        return await Repository.UpdateAsync(entity, true);
    }

    public virtual Task DeleteAsync(string connectionId)
    {
        return Repository.DeleteAsync(connectionId);
    }

    public virtual async Task<int> ClearUnactiveAsync()
    {
        Expression<Func<Connection, bool>> predicate = x => x.ActiveTime < Clock.Now.AddSeconds(-Config.InactiveSeconds);

        var count = await Repository.CountAsync(predicate);

        if (count == 0)
        {
            return 0;
        }
        await Repository.DeleteAsync(predicate);

        return count;
    }

}
