using IczpNet.Chat.Enums;
using System;
using System.Data;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageReports;

public interface IMessageReportRepository
{
    Task IncrementAsync(Guid sessionId, MessageTypes messageType, string dateBucketFormat = "yyyyMMdd");


    Task FlushToDbAsync(DataTable dataTable);

    /// <summary>
    /// 创建数据结构 MessageReportMergeType
    /// </summary>
    /// <returns></returns>
    Task EnsureMessageReportMergeTypeAsync();
}