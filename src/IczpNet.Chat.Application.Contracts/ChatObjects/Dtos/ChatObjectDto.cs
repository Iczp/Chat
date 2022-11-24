using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.ChatObjects.Dtos
{
    public class ChatObjectDto : BaseDto
    {
        public virtual string Name { get; set; }

        public virtual string Code { get; set; }
    }
}
