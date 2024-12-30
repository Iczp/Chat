using IczpNet.AbpTrees;
using System;

namespace IczpNet.Chat.EntryNames;

public interface IEntryNameManager : ITreeManager<EntryName, Guid, TreeInfo<Guid>>
{
}
