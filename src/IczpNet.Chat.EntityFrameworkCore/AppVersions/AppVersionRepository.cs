using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Repositories;
using System;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.AppVersions;

/// <inheritdoc />
public class AppVersionRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : ChatRepositoryBase<AppVersion, Guid>(dbContextProvider), IAppVersionRepository
{
    
}

