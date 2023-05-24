using System;

namespace IczpNet.Chat.Words.Dtos;

public class WordCreateInput : WordUpdateInput
{
    public virtual string Value { get; set; }
}
