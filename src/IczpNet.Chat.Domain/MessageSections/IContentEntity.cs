namespace IczpNet.Chat.MessageSections
{
    public interface IContentEntity : IContent
    {
        string GetBody();

        long GetSize();
    }
}
