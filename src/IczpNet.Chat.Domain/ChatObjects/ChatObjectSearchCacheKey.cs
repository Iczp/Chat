namespace IczpNet.Chat.ChatObjects;

public class ChatObjectSearchCacheKey(string keyword)
{
    public string Keyword { get; set; } = keyword;

    public override string ToString()
    {
        return $"ChatObjects:Keywords:{Keyword}";
    }
}
