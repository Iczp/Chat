using System;

namespace IczpNet.Chat.AppVersions;

public interface IAppVersionAppService : ICrudChatAppService<AppVersionDetailDto, AppVersionDto, Guid, AppVersionGetListInput, AppVersionCreateInput, AppVersionUpdateInput>
{

}
