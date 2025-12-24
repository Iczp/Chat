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
    /// <param name="granularity">Month | Day | Hour</param>
    /// <param name="dateBucket"></param>
    /// <returns></returns>
    Task FlushAsync(string granularity, long? dateBucket = null);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task FlushMonthAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task FlushDayAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task FlushHourAsync();

    /// <summary>
    /// 补偿
    /// </summary>
    /// <returns></returns>
    Task CompensateAsync();

}
