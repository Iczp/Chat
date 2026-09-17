namespace IczpNet.Chat.Ai;

public class AiRunExecutionResult
{
    public bool Success { get; set; }
    public long? OutputMessageId { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }

    public static AiRunExecutionResult Ok(long? outputMessageId = null) => new()
    {
        Success = true,
        OutputMessageId = outputMessageId
    };

    public static AiRunExecutionResult Fail(string errorCode, string errorMessage) => new()
    {
        Success = false,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage
    };
}
