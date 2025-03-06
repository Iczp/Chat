using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Microsoft.Extensions.Logging;
using IczpNet.Chat.ServerHosts;
using Volo.Abp.Uow;
using System.Threading;

namespace IczpNet.Chat.Connections;

/// <inheritdoc />
public class ConnectionManager(
    IRepository<Connection, string> repository,
    IOptions<ConnectionOptions> options,
    IRepository<ServerHost, string> serverHostRepository) : DomainService, IConnectionManager
{
    protected IRepository<Connection, string> Repository { get; } = repository;
    protected IRepository<ServerHost, string> ServerHostRepository { get; } = serverHostRepository;
    protected ConnectionOptions Config { get; } = options.Value;

    /// <inheritdoc />
    public virtual async Task<ConnectionOptions> GetConfigAsync()
    {
        await Task.Yield();
        return Config;
    }

    /// <inheritdoc />
    public virtual async Task<Connection> CreateAsync(Connection connection, CancellationToken token = default)
    {
        if (!connection.ServerHostId.IsNullOrWhiteSpace())
        {
            var serverHost = await ServerHostRepository.FindAsync(connection.ServerHostId, cancellationToken: token);
            serverHost ??= await ServerHostRepository.InsertAsync(new ServerHost(connection.ServerHostId), cancellationToken: token);
            connection.ServerHost = serverHost;
        }

        var entity = await Repository.InsertAsync(connection, autoSave: true, cancellationToken: token);
        // 
        return entity;
    }

    /// <inheritdoc />
    public Task<int> GetOnlineCountAsync(DateTime currentTime, CancellationToken token = default)
    {
        return Repository.CountAsync(x => x.ActiveTime > currentTime.AddSeconds(-Config.InactiveSeconds), cancellationToken: token);
    }

    /// <inheritdoc />
    public Task<Connection> GetAsync(string connectionId, CancellationToken token = default)
    {
        return Repository.GetAsync(connectionId, cancellationToken: token);
    }

    /// <inheritdoc />
    [UnitOfWork]
    public virtual async Task<Connection> UpdateActiveTimeAsync(string connectionId, CancellationToken token = default)
    {
        //using var uow = UnitOfWorkManager.Begin();

        var entity = await Repository.FindAsync(connectionId, cancellationToken: token);

        if (entity == null)
        {
            Logger.LogWarning($"No such connectionId:{connectionId}");
            return entity;
        }
        entity.SetActiveTime(Clock.Now);

        return await Repository.UpdateAsync(entity, true, token);
    }

    /// <inheritdoc />
    public virtual Task RemoveAsync(string connectionId, CancellationToken token = default)
    {
        return Repository.DeleteAsync(connectionId, cancellationToken: token);
    }

    /// <inheritdoc />
    public virtual async Task<int> ClearUnactiveAsync(CancellationToken token = default)
    {
        Expression<Func<Connection, bool>> predicate = x => x.ActiveTime < Clock.Now.AddSeconds(-Config.InactiveSeconds);

        var count = await Repository.CountAsync(predicate, cancellationToken: token);

        if (count == 0)
        {
            return 0;
        }
        await Repository.DeleteAsync(predicate, cancellationToken: token);

        return count;
    }

}
