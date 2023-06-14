using IczpNet.Chat.Localization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace IczpNet.Chat;

[Area(ChatRemoteServiceConsts.ModuleName)]
[Route($"/api/chat/[Controller]/[Action]")]
[ApiExplorerSettings(GroupName = ChatRemoteServiceConsts.ModuleName)]
public abstract class ChatController : AbpControllerBase
{
    protected ChatController()
    {
        LocalizationResource = typeof(ChatResource);
    }
}
