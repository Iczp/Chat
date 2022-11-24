using IczpNet.Chat.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace IczpNet.Chat;

public abstract class ChatController : AbpControllerBase
{
    protected ChatController()
    {
        LocalizationResource = typeof(ChatResource);
    }
}
