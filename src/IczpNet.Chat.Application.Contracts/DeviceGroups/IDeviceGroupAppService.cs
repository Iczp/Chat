using System;

namespace IczpNet.Chat.DeviceGroups;

public interface IDeviceGroupAppService : ICrudChatAppService<DeviceGroupDetailDto, DeviceGroupDto, Guid, DeviceGroupGetListInput, DeviceGroupCreateInput, DeviceGroupUpdateInput>
{

}
