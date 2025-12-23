using System;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.MessageStats;

public class DataBucket : DomainService, IDataBucket
{
    public long Create(DateTime dateTime, string format = "yyyyMMdd")
    {
        return long.Parse(dateTime.ToString(format));
    }
}
