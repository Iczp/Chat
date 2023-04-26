using IczpNet.Chat.Localization;
using Microsoft.AspNetCore.Mvc;

namespace IczpNet.Chat.BaseAppServices;

[ApiExplorerSettings(GroupName = ChatRemoteServiceConsts.ModuleName)]
public abstract class ChatManagementAppService : ChatAppService
{

    protected ChatManagementAppService()
    {
        LocalizationResource = typeof(ChatResource);
        ObjectMapperContext = typeof(ChatApplicationModule);
    }
}
