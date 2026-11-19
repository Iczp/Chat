namespace IczpNet.Chat.UrlNormalizers;

public interface IUrlNormalizer
{
    string Normalize(string url);

    string ToRelative(string url);
}
