using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using System.Threading.Tasks;
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
        protected virtual IContentProvider GetContentProvider(string providerName)
        {
            var providerType = ContentResolver.GetProviderTypeOrDefault(providerName);
            return LazyServiceProvider.LazyGetService(providerType) as IContentProvider;
        }

        //public virtual async Task<MessageInfo<TextContentInfo>> SendMessageAsync(MessageInput<TextContentInfo> input)
        //{
        //    var provider = GetContentProvider(TextContentProvider.Name);
        //    var content = await provider.Create<TextContentInfo, TextContent>(input.Content);
        //    return await MessageManager.CreateMessageAsync<TextContentInfo>(input, x => x.SetMessageContent(content));
        //}
    }
}
