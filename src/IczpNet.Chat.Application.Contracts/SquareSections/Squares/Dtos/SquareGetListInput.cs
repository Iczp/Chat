using IczpNet.Chat.BaseDtos;
using System.ComponentModel;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SquareSections.Squares.Dtos;

public class SquareGetListInput : BaseGetListInput
{
    [DefaultValue(null)]
    public virtual List<Guid> CategoryIdList { get; set; }
    public bool IsImportChildCategory { get; set; }
}
