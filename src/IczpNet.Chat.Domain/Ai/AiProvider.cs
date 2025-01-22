using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.Ai;

public abstract class AiProvider : DomainService, IAiProvider
{
    /// <inheritdoc />
    public virtual string GetProviderName() => GetType().FullName;
    /// <inheritdoc />
    public abstract string GetModel();
    protected IMessageManager MessageManager => LazyServiceProvider.LazyGetRequiredService<IMessageManager>();
    protected IMessageSender MessageSender => LazyServiceProvider.LazyGetRequiredService<IMessageSender>();
    protected ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();
    protected IMessageRepository MessageRepository => LazyServiceProvider.LazyGetRequiredService<IMessageRepository>();
    protected Type ObjectMapperContext { get; set; }
    protected IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetService<IObjectMapper>(provider =>
        ObjectMapperContext == null
            ? provider.GetRequiredService<IObjectMapper>()
            : (IObjectMapper)provider.GetRequiredService(typeof(IObjectMapper<>).MakeGenericType(ObjectMapperContext)));
    public abstract Task HandleAsync(long messageId);
}
