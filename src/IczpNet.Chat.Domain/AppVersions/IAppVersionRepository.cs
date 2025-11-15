using System;
using Volo.Abp.Domain.Repositories;


namespace IczpNet.Chat.AppVersions;

public interface IAppVersionRepository : IRepository<AppVersion, Guid>
{

}
