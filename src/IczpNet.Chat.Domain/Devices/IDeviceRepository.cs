using System;
using Volo.Abp.Domain.Repositories;


namespace IczpNet.Chat.Devices;

public interface IDeviceRepository : IRepository<Device, Guid>
{

}
