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
        protected virtual IContentProvider GetContentProvider(string providerName)
        {
            var providerType = ContentResolver.GetProviderTypeOrDefault(providerName);
            return LazyServiceProvider.LazyGetService(providerType) as IContentProvider;
        }
    }
}
