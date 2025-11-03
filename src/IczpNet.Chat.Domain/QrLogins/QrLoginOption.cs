namespace IczpNet.Chat.QrLogins;

public class QrLoginOption
{

    public string QrText { get; set; } = "gotoim://scan-login?code={code}";

    public int ExpiredSeconds { get; set; } = 60;
    public string DistributedCacheKey { get; set; } = "QrLoginDistributedCacheKey";
}
