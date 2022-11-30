using IczpNet.AbpTrees;
using System;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SquareSections.SquareCategorys;

public class SquareCategoryManager : TreeManager<SquareCategory, Guid>, ISquareCategoryManager
{
    public SquareCategoryManager(IRepository<SquareCategory, Guid> repository) : base(repository)
    {
    }
}
