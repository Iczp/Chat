using IczpNet.Chat.DataFilters;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

    protected async Task<MessageInfo<TextContentInfo>> SendTextAsync(SessionUnit replySessionUnit, long quoteMessageId, TextContentInfo content)
    {
        var dto = await MessageSender.SendTextAsync(replySessionUnit, new MessageInput<TextContentInfo>()
        {
            QuoteMessageId = quoteMessageId,
            Content = content
        });

        //var dto = await MessageSender.SendHtmlAsync(replySessionUnit, new MessageInput<HtmlContentInfo>()
        //{
        //    QuoteMessageId = messageId,
        //    Content = new HtmlContentInfo()
        //    {
        //        Content = result,
        //        EditorType = EditorTypes.MarkDown,
        //        Title = GetModel(),
        //    }
        //});

        Logger.LogInformation($"回复消息：{dto}");

        return dto;
    }
}
