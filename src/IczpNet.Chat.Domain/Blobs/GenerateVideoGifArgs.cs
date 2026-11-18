using System;

namespace IczpNet.Chat.Blobs;

[Serializable]
public class GenerateVideoGifArgs
{
    public long MessageId { get; set; }

    public Guid VideoBlobId { get; set; }
}
