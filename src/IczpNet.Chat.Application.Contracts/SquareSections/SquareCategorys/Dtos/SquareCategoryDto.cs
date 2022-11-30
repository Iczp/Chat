using System;
using Volo.Abp.Application.Dtos;
namespace IczpNet.Chat.SquareSections.SquareCategorys.Dtos;

/// <summary>
/// SquareCategory Dto
/// </summary>
public class SquareCategoryDto : SquareCategoryInfo, IEntityDto<Guid>
{

    public virtual string Description { get; set; }
}
