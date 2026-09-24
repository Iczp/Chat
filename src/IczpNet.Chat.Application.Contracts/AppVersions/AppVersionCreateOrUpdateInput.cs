using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.AppVersions;

/// <summary>
/// 鏂板鎴栦慨鏀?
/// </summary>
[Serializable]
public class AppVersionCreateOrUpdateInput : AppVersionCreateInput, IEntityDto<Guid?>
{
    /// <summary>
    /// 
    /// </summary>
    public Guid? Id { get; set; }

    public virtual string GetObjectKey() => $"{Id}";
}