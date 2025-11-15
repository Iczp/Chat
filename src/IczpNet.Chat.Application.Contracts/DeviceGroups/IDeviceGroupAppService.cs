using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.DeviceGroups;

public interface IDeviceGroupAppService : ICrudChatAppService<DeviceGroupDetailDto, DeviceGroupDto, Guid, DeviceGroupGetListInput, DeviceGroupCreateInput, DeviceGroupUpdateInput>
{
    Task<int> SetDevicesAsync(Guid id, List<Guid> deviceIdList);
}
