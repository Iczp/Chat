using IczpNet.Chat.BaseEntities;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Blobs;

public class BlobContent : BaseEntity
{
    public virtual Guid BlobId { get; set; }

    [ForeignKey(nameof(BlobId))]
    public virtual Blob Blob { get; set; }

    public virtual byte[] Content { get; set; }

    public override object[] GetKeys()
    {
        return new object[] { BlobId };
    }
}
