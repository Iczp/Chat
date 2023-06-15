using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.MessageSections
{
    public class TemplateManager : DomainService, ITemplateManager
    {
        protected IMessageManager MessageManager { get; }
        protected IContentResolver ContentResolver { get; }

        public TemplateManager(
            IMessageManager messageManager,
            IContentResolver contentResolver)
        {
            MessageManager = messageManager;
            ContentResolver = contentResolver;
        }
        protected virtual IContentProvider GetContentProvider(MessageTypes messageType)
        {
            var providerType = ContentResolver.GetProviderTypeOrDefault(messageType);
            return LazyServiceProvider.LazyGetService(providerType) as IContentProvider;
        }

        //public virtual async Task<MessageInfo<TextContentInfo>> SendMessageAsync(MessageInput<TextContentInfo> input)
        //{
        //    var provider = GetContentProvider(TextContentProvider.Name);
        //    var content = await provider.CreateAsync<TextContentInfo, TextContent>(input.Content);
        //    return await MessageManager.CreateMessageAsync<TextContentInfo>(input, x => x.SetMessageContent(content));
        //}
    }
}
