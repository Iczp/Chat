using IczpNet.Chat.Management.Mottos.Dtos;

namespace IczpNet.Chat.Management.ChatObjects.Dtos
{
    public class ChatObjectDto : ChatObjectSimpleDto
    {

        public MottoSimpleDto Motto { get; set; }

        public string NameSpellingAbbreviation { get; set; }
    }
}
