using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.AppVersions;

/// <summary>
/// 新增或修改
/// </summary>
[Serializable]
public class AppVersionCreateOrUpdateInput : AppVersionCreateInput, IEntityDto<Guid?>
{
    /// <summary>
    /// 
    /// </summary>
    public Guid? Id { get; set; }
}