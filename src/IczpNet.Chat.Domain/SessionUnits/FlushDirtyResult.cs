namespace IczpNet.Chat.SessionUnits;

public class FlushDirtyResult
{
    public long Total { get;  set; }
    public int Execute { get;  set; }
    public int Affect { get;  set; }
    public long Remaining { get;  set; }
    public long Elapsed { get;  set; }
}
