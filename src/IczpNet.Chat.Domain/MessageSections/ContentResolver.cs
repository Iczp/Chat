using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.MessageSections
{
    public class ContentResolver : IContentResolver, ISingletonDependency
    {
        public string GetProvider(string name)
        {
            throw new NotImplementedException();
        }
    }
}
