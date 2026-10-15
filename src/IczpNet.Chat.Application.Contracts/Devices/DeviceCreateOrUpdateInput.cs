using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Devices;

/// <summary>
/// 鏂板鎴栦慨鏀?
/// </summary>
[Serializable]
public class DeviceCreateOrUpdateInput : DeviceUpdateInput, IEntityDto<Guid?>
{
    [System.ComponentModel.DataAnnotations.MaxLength(256)]
    [Volo.Abp.Auditing.DisableAuditing]
    public string RegistrationProof { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Guid? Id { get; set; }

    public virtual string GetObjectKey() => $"{Id}";
}