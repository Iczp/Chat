using System;

namespace IczpNet.Chat.QrCodes.Dtos;

public class QrCodeDetailDto : QrCodeDto
{
    public virtual bool IsExpired { get; set; }

    public virtual string OwnerName { get; set; }

    public virtual string OwnerCode { get; set; }

    public virtual string OwnerPortrait { get; set; }

    public virtual string OwnerThumbnail { get; set; }

    public virtual string InviterName { get; set; }
}
