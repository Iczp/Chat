using IczpNet.Chat.EntryValues.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace IczpNet.Chat.Entries.Dtos;

public class EntryInput
{
    public Guid NameId { get; set; }

    public List<EntryValueInput> Values { get; set; }

}
