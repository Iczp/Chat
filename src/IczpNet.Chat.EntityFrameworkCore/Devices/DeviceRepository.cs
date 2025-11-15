using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Repositories;
using System;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.Devices;

/// <inheritdoc />
public class DeviceRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : ChatRepositoryBase<Device, Guid>(dbContextProvider), IDeviceRepository
{
    
}

