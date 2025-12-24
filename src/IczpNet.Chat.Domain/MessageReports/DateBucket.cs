using System;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.MessageReports;

public class DateBucket : DomainService, IDateBucket
{
    public long Create(DateTime dateTime, string format = "yyyyMMdd")
    {
        return long.Parse(dateTime.ToString(format));
    }
}
