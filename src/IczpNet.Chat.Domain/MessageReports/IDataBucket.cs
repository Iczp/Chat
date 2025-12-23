using System;

namespace IczpNet.Chat.MessageStats;

public interface IDataBucket
{
    long Create(DateTime dateTime, string format = "yyyyMMdd");
}
