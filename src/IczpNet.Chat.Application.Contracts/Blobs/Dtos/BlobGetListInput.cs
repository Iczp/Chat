using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.Blobs.Dtos;

public class BlobGetListInput : GetListInput
{

    public virtual bool? IsStatic { get; set; }

    public virtual bool? IsPublic { get; set; }

    public virtual bool? IsOption { get; set; }
}
