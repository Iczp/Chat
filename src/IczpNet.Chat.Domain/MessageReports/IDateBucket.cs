using IczpNet.Chat.Enums;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageReports;

public interface IDateBucket
{
    Task<DateTime> GetBusinessTimeAsync(DateTime dateTime);

    Task<long> CreateAsync(DateTime dateTime, string format = "yyyyMMdd");

    long Create(DateTime dateTime, string format = "yyyyMMdd");

    long Create(DateTime dateTime, MessageReportTypes reportType);
}
