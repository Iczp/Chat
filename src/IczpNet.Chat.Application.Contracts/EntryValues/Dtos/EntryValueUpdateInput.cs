using IczpNet.Chat.BaseDtos;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.EntryValues.Dtos;

public class EntryValueUpdateInput : BaseInput
{
    public virtual string Value { get; set; }

    public virtual bool IsStatic { get; set; }

    public virtual bool IsPublic { get; set; }


}
