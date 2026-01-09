using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.MessageReports;

public interface IDateBucket
{
    long Create(DateTime dateTime, string format = "yyyyMMdd");

    long Create(DateTime dateTime, MessageReportTypes reportType);
}
