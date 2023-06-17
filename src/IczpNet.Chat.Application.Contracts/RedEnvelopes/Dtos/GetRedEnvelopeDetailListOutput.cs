
using IczpNet.Chat.ChatObjects.Dtos;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.RedEnvelopes.Dtos
{
    /// <summary>
    /// GetRedEnvelopeDetailListOutput
    /// </summary>
    public class GetRedEnvelopeDetailListOutput : PagedResultDto<RedEnvelopeDetailResult>
    {
        /// <summary>
        /// 红包内容
        /// </summary>
        public RedEnvelopeContentSimpleDto RedEnvelopeContent { get; set; }
        /// <summary>
        /// 发红包的人
        /// </summary>
        public List<ChatObjectSimpleDto> UserList { get; set; }
    }
}
