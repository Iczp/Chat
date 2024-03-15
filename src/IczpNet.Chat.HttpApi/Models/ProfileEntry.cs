using System;

namespace IczpNet.Chat.Models;

[Serializable]
public class ProfileEntry
{
    public string Name { get; set; }
    public string Tag { get; set; }
    public string Value { get; set; }
}
