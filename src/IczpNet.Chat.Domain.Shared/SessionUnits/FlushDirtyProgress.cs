namespace IczpNet.Chat.SessionUnits;

public class FlushDirtyProgress
{
    /// <summary>
    /// 总数量
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// Job数量
    /// </summary>
    public long JobCount { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int Completed { get; set; }

    /// <summary>
    /// Job完成数量
    /// </summary>
    public int JobCompleted { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreationTime { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public long? LastModificationTime { get; set; }
}
