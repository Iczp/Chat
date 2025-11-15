using System;

namespace IczpNet.Chat.Devices;

public interface IDeviceAppService : ICrudChatAppService<DeviceDetailDto, DeviceDto, Guid, DeviceGetListInput, DeviceCreateInput, DeviceUpdateInput>
{

}
