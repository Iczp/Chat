using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ShopWaiters.Dtos
{
    public class ShopWaiterGetListInput: PagedAndSortedResultRequestDto
    {
        public virtual long ShopKeeperId { get; set; }
    }
}
