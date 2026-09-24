using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.DeviceGroups;

/// <summary>
/// 鏂板鎴栦慨鏀?
/// </summary>
[Serializable]
public class DeviceGroupCreateOrUpdateInput : DeviceGroupCreateInput, IEntityDto<Guid?>
{
    /// <summary>
    /// 
    /// </summary>
    public Guid? Id { get; set; }

    public virtual string GetObjectKey() => $"{Id}";
}