using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Devices;

/// <summary>
/// 新增或修改
/// </summary>
[Serializable]
public class DeviceCreateOrUpdateInput : DeviceCreateInput, IEntityDto<Guid?>
{
    /// <summary>
    /// 
    /// </summary>
    public Guid? Id { get; set; }
}