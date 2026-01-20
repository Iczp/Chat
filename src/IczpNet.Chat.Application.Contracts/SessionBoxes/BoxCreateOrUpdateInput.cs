using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionBoxes;

/// <summary>
/// 新增或修改
/// </summary>
[Serializable]
public class BoxCreateOrUpdateInput : BoxCreateInput, IEntityDto<Guid?>
{
    /// <summary>
    /// 
    /// </summary>
    public Guid? Id { get; set; }
}