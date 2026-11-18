using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.QrCodes.Dtos;

public class QrCodeGetListInput : GetListInput
{
    public virtual long? OwnerId { get; set; }

    public virtual long? InviterId { get; set; }

    public virtual QrCodeScenes? Scene { get; set; }

    public virtual bool? IsPermanent { get; set; }

    public virtual bool? IsEnabled { get; set; }
}
