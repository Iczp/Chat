namespace IczpNet.Chat.ShopWaiters.Dtos
{
    public class ShopWaiterCreateInput : ShopWaiterUpdateInput
    {
        public virtual long ShopKeeperId { get; set; }
    }
}
