using IczpNet.Chat.Management.BaseDtos;


namespace IczpNet.Chat.Management.ChatObjects.Dtos;

public class ChatObjectUpdateInput : BaseTreeInputDto<long>
{
    public virtual string Code { get; set; }

    public virtual string Description { get; set; }
}
