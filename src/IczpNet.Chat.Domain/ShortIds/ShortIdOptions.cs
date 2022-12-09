using shortid.Configuration;

namespace IczpNet.Chat.ShortIds
{
    public class ShortIdOptions
    {
        public bool UseNumbers { get; set; }

        public bool UseSpecialCharacters { get; set; }

        public int Length { get; set; } = 8;
    }
}
