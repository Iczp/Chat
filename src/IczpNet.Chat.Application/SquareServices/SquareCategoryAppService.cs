using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SquareSections.SquareCategorys;
using IczpNet.Chat.SquareSections.SquareCategorys.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SquareServices
{
    public class SquareCategoryAppService
        : CrudTreeChatAppService<
            SquareCategory,
            Guid,
            SquareCategoryDetailDto,
            SquareCategoryDto,
            SquareCategoryGetListInput,
            SquareCategoryCreateInput,
            SquareCategoryUpdateInput,
            SquareCategoryInfo>,
        ISquareCategoryAppService
    {
        public SquareCategoryAppService(IRepository<SquareCategory, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<SquareCategory>> CreateFilteredQueryAsync(SquareCategoryGetListInput input)
        {

            Assert.If(!input.IsEnabledParentId && input.ParentId.HasValue, "When [IsEnabledParentId]=false,then [ParentId] != null");

            return (await Repository.GetQueryableAsync())
                //.WhereIf(input.Depth.HasValue, (x) => x.Depth == input.Depth)
                .WhereIf(input.IsEnabledParentId, (x) => x.ParentId == input.ParentId)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.Contains(input.Keyword));


        }
    }
}
