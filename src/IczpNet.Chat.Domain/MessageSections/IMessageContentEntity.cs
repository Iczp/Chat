namespace IczpNet.Chat.MessageSections
{
    public interface IMessageContentEntity
    {
        string GetBody();

        long GetSize();
    }
}
