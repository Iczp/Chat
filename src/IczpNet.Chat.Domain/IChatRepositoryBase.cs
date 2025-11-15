using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat;

public interface IChatRepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
{
}

public interface IChatRepositoryBase<TEntity, TKey> : IRepository<TEntity,TKey> where TEntity : class, IEntity<TKey>
{
}