using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.AppVersions;

public interface IAppVersionAppService : ICrudChatAppService<AppVersionDetailDto, AppVersionDto, Guid, AppVersionGetListInput, AppVersionCreateInput, AppVersionUpdateInput>
{

    Task<int> SetGroupsAsync(Guid id, List<Guid> groupIdList);

    /// <summary>
    /// 最新版
    /// </summary>

    Task<AppVersionDetailDto> GetLatest(GetLatestInput input);

}
