using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;



namespace IczpNet.Chat.MessageSections;

public abstract class ContentProviderBase<TEntity> : DomainService, IContentProvider where TEntity : class, IEntity<Guid>
{
    public abstract string ProviderName { get; }
    protected IRepository<TEntity, Guid> Repository { get; }
    public abstract Task<IContentInfo> GetContentInfoAsync(long messageId);
    public abstract Task<TOutput> CreateAsync<TInput, TOutput>(TInput input);
    protected Type ObjectMapperContext { get; set; }
    protected IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetService<IObjectMapper>(provider =>
        ObjectMapperContext == null
            ? provider.GetRequiredService<IObjectMapper>()
            : (IObjectMapper)provider.GetRequiredService(typeof(IObjectMapper<>).MakeGenericType(ObjectMapperContext)));



    protected ContentProviderBase(IRepository<TEntity, Guid> repository)
    {
        Repository = repository;
    }
}
