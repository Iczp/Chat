using IczpNet.Chat.RedEnvelopes.Dtos;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.RedEnvelopes;

public interface IRedEnvelopeAppService
{
    /// <summary>
    /// 抢红包
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="redEnvelopeContentId"></param>
    /// <returns></returns>
    Task<GrabRedEnvelopeOutput> Grab(long messageId, Guid redEnvelopeContentId);

    /// <summary>
    /// 红包领取记录
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<GetRedEnvelopeDetailListOutput> GetDetailList(GetRedEnvelopeDetailListInput input);

    /// <summary>
    /// 我的红包记录
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<GetMyRedEnvelopeDetailListOutput> GetMyDetailList(GetMyRedEnvelopeDetailListInput input);
}
