using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageReports;

public interface IMessageReportManager
{

    /// <summary>
    /// 创建数据结构
    /// </summary>
    /// <returns></returns>
    Task InitializationAsync();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task StatAsync(Message message);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reportType">Month | Day | Hour</param>
    /// <param name="dateBucket"></param>
    /// <returns></returns>
    Task<bool> FlushAsync(MessageReportTypes reportType, long? dateBucket = null);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<bool> FlushMonthAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<bool> FlushDayAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<bool> FlushHourAsync();

    /// <summary>
    /// 补偿
    /// </summary>
    /// <returns></returns>
    Task CompensateAsync();

}
