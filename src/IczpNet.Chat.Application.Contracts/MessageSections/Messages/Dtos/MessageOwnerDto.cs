using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public class MessageByDateDto 
{
    /// <summary>
    /// Date
    /// </summary>
    public virtual DateTime Date { get; set; }

    /// <summary>
    /// Count
    /// </summary>
    public virtual int Count { get; set; }

    /// <summary>
    /// MessageIdList
    /// </summary>
    public virtual List<long> MessageIdList { get; set; }

}
