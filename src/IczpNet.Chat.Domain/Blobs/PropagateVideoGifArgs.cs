using System;

namespace IczpNet.Chat.Blobs;

[Serializable]
public class PropagateVideoGifArgs
{
    public long SourceMessageId { get; set; }

    public Guid VideoContentId { get; set; }

    public Guid GifBlobId { get; set; }

    public string GifUrl { get; set; }
}
