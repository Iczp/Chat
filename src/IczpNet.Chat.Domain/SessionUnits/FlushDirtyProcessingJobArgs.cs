using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits;

[Serializable]
public class FlushDirtyProcessingJobArgs
{
    public string ProcessingKey { get; set; }

    public int ScanSize { get; set; } = 5000;

    public int JobSize { get; set; } = 1000;
    public int Count { get; set; }
    public int JobIndex { get; set; }
    public int JobTotalCunt { get; set; }
    public List<Guid> SessionUnitIds { get; set; }

    public override string ToString()
    {
        return base.ToString();
    }
}