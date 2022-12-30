using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Etos
{
    public class SendDataEto
    {
        public List<Guid> TargetIdList { get; set; }

        public object Data { get; set; }

        public SendDataEto() { }

        public SendDataEto(List<Guid> targetIdList, object data)
        {
            TargetIdList = targetIdList;
            Data = data;
        }

    }
}
