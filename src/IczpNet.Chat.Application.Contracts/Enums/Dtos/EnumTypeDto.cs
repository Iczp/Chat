using System.Collections.Generic;

namespace IczpNet.Chat.Enums.Dtos;

public class EnumTypeDto
{
    public string Id { get; set; }

    public string Name { get; set; }

    public List<EnumDto> Items { get; set; } = [];
}
