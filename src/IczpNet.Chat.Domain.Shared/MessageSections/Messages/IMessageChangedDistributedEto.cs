﻿namespace IczpNet.Chat.MessageSections.Messages;

public interface IMessageChangedDistributedEto
{
    /// <summary>
    /// Command
    /// </summary>
    string Command { get; set; }
    /// <summary>
    /// MessageId
    /// </summary>
    long MessageId { get; set; }

    /// <summary>
    /// CacheKey
    /// </summary>
    string CacheKey { get; set; }

    /// <summary>
    /// HostName
    /// </summary>
    string HostName { get; set; }
}
