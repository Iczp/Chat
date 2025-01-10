using Volo.Abp.Data;

namespace IczpNet.Chat.ClientConfigs;

public static class ClientConfigExtensions
{
    private const string TitlePropertyName = "Title";

    public static void SetTitle(this ClientConfig config, string title)
    {
        config.SetProperty(TitlePropertyName, title);
    }

    public static string GetTitle(this ClientConfig config)
    {
        return config.GetProperty<string>(TitlePropertyName);
    }
}
