using System;

namespace IczpNet.Chat.MessageReports;

public interface IDateBucket
{
    long Create(DateTime dateTime, string format = "yyyyMMdd");

}
