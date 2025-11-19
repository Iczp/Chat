using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Devices;

public interface IDeviceAppService : ICrudChatAppService<DeviceDetailDto, DeviceDto, Guid, DeviceGetListInput, DeviceCreateInput, DeviceUpdateInput>
{
    Task<int> SetGroupsAsync(Guid id, List<Guid> groupIdList);

    Task<DeviceDetailDto> RegisterAsync(DeviceCreateOrUpdateInput input);

    Task<PagedResultDto<DeviceDto>> GetListByCurrentUserAsync();

    Task<DeviceDetailDto> GetByDeviceIdAsync(string deivceId);
}
