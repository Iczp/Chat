using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Repositories;
using System;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.DeviceGroups;

/// <inheritdoc />
public class DeviceGroupRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : ChatRepositoryBase<DeviceGroup, Guid>(dbContextProvider), IDeviceGroupRepository
{
    
}

