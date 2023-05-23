using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Words.Dtos
{
    public class WordDto : EntityDto<string>
    {

        public virtual bool IsEnabled { get; set; }

        public virtual bool IsDirty { get; set; }
    }
}
