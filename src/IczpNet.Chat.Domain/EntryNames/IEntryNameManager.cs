using IczpNet.AbpTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IczpNet.Chat.EntryNames
{
    public interface IEntryNameManager : ITreeManager<EntryName, Guid>
    {
    }
}
