using IczpNet.Chat.Localization;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat;

public abstract class ChatAppService : ApplicationService
{
    protected ChatAppService()
    {
        LocalizationResource = typeof(ChatResource);
        ObjectMapperContext = typeof(ChatApplicationModule);
    }
}
