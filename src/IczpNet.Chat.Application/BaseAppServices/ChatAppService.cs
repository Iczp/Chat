using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Localization;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.BaseAppServices;

public abstract class ChatAppService : ApplicationService
{
    protected ICurrentChatObject CurrentChatObject => LazyServiceProvider.LazyGetRequiredService<ICurrentChatObject>();
    protected ChatAppService()
    {
        LocalizationResource = typeof(ChatResource);
        ObjectMapperContext = typeof(ChatApplicationModule);
    }
}
