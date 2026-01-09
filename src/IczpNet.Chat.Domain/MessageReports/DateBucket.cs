using IczpNet.Chat.Enums;
using System;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.MessageReports;

public class DateBucket : DomainService, IDateBucket
{
    public long Create(DateTime dateTime, string format = "yyyyMMdd")
    {
        return long.Parse(dateTime.ToString(format));
    }

    public long Create(DateTime dateTime, MessageReportTypes reportType)
    {
        return reportType switch
        {
            MessageReportTypes.Month => Create(dateTime, "yyyyMM"),
            MessageReportTypes.Day => Create(dateTime, "yyyyMMdd"),
            MessageReportTypes.Hour => Create(dateTime, "yyyyMMddhh"),
            _ => throw new NotImplementedException(),
        };
    }
}
