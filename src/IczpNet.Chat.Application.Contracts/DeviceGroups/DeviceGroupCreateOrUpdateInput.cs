using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.DeviceGroups;

/// <summary>
/// 新增或修改
/// </summary>
[Serializable]
public class DeviceGroupCreateOrUpdateInput : DeviceGroupCreateInput, IEntityDto<Guid?>
{
    /// <summary>
    /// 
    /// </summary>
    public Guid? Id { get; set; }
}