using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.ShopWaiters.Dtos;

public class ShopWaiterGetListInput : GetListInput
{
    /// <summary>
    /// 
    /// </summary>
    public virtual bool IsContainsShopKeeper { get; set; } = false;

    /// <summary>
    /// 掌柜Id[聊天对象]
    /// </summary>
    public virtual long ShopKeeperId { get; set; }
}
