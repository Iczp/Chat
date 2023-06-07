using IczpNet.AbpTrees;
using System;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.EntryNames
{
    public class EntryNameManager : TreeManager<EntryName, Guid>, IEntryNameManager
    {
        public EntryNameManager(
            IRepository<EntryName, Guid> repository) : base(repository)
        {
        }
    }
}
