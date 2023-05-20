
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;



namespace IczpNet.Chat.MessageSections
{
    public abstract class ContentProviderBase : DomainService, IContentProvider
    {
        public abstract Task<IContentInfo> GetContent(long messageId);
        public abstract Task<TOutput> Create<TInput, TOutput>(TInput input);

        protected Type ObjectMapperContext { get; set; }
        protected IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetService<IObjectMapper>(provider =>
            ObjectMapperContext == null
                ? provider.GetRequiredService<IObjectMapper>()
                : (IObjectMapper)provider.GetRequiredService(typeof(IObjectMapper<>).MakeGenericType(ObjectMapperContext)));

    }
}
