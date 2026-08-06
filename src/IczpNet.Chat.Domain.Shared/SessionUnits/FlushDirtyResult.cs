namespace IczpNet.Chat.SessionUnits;

public class FlushDirtyResult
{
    public long Total { get; set; }

    /// <summary>
    /// 本次取出的数量
    /// </summary>
    public int Execute { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int JobCount { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public long Elapsed { get; set; }

}
