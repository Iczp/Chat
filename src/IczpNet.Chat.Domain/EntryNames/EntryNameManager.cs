using IczpNet.AbpTrees;
using System;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.EntryNames;

public class EntryNameManager(
    IRepository<EntryName, Guid> repository) : TreeManager<EntryName, Guid, TreeInfo<Guid>>(repository), IEntryNameManager
{
}
