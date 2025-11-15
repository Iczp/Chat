using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.Devices;

public interface IDeviceAppService : ICrudChatAppService<DeviceDetailDto, DeviceDto, Guid, DeviceGetListInput, DeviceCreateInput, DeviceUpdateInput>
{
    Task<int> SetGroupsAsync(Guid id, List<Guid> groupIdList);
}
