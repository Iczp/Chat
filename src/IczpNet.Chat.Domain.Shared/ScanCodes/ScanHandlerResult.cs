namespace IczpNet.Chat.ScanCodes;

public class ScanHandlerResult
{
    public bool Success { get; set; }

    public string Action { get; set; }

    public string Message { get; set; }

    public string Data { get; set; }

    public static ScanHandlerResult Ok(string action, string msg = null, string data = null)
        => new()
        {
            Action = action,
            Success = true,
            Message = msg,
            Data = data
        };

    public static ScanHandlerResult Fail(string msg)
        => new()
        {
            Success = false,
            Message = msg
        };
}