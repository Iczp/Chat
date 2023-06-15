using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace IczpNet.Chat.MessageSections
{
    public interface IContentProvider
    {
        string ProviderName { get; }

        Task<IContentInfo> GetContentInfoAsync(long messageId);

        Task<TOutput> CreateAsync<TInput, TOutput>(TInput input);
    }

    public interface IContentProvider<TEntity, TInput, TOutput> 
        where TEntity: class,IEntity<Guid>
        where TInput : IContentInfo
        where TOutput : IContentInfo
    {
        Task<TEntity> CreateAsync(TInput input);

        Task<TOutput> MapToInfoAsync(TEntity entity);
    }

}
