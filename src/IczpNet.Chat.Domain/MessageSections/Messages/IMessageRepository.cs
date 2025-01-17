using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.MessageSections.Messages;

public interface IMessageRepository : IRepository<Message, long>
{
    /// <summary>
    /// 增加已读次数
    /// </summary>
    /// <param name="messageIdList"></param>
    /// <returns></returns>
    Task<int> IncrementReadedCountAsync(List<long> messageIdList);

    /// <summary>
    /// 增加打开次数
    /// </summary>
    /// <param name="messageIdList"></param>
    /// <returns></returns>
    Task<int> IncrementOpenedCountAsync(List<long> messageIdList);

    /// <summary>
    /// 增加收藏次数
    /// </summary>
    /// <param name="messageIdList"></param>
    /// <returns></returns>
    Task<int> IncrementFavoritedCountAsync(List<long> messageIdList);

    /// <summary>
    /// 增加引用次数
    /// </summary>
    /// <param name="messageIdList"></param>
    /// <returns></returns>
    Task<int> IncrementQuoteCountAsync(List<long> messageIdList);

    /// <summary>
    /// 增加转发次数
    /// </summary>
    /// <param name="messageIdList"></param>
    /// <returns></returns>
    Task<int> IncrementForwardCountAsync(List<long> messageIdList);

    //Task<int> IncrementRecorderAsync(List<long> messageIdList);

}
