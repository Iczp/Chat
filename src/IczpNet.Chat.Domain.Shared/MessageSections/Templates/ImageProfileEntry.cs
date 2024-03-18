using System;

namespace IczpNet.Chat.MessageSections.Templates;

[Serializable]
public class ImageProfileEntry
{
    public string Name { get; set; }
    public string Tag { get; set; }
    public string Value { get; set; }
}
