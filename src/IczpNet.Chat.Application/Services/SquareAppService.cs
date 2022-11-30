using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SquareSections.SquareCategorys;
using IczpNet.Chat.SquareSections.Squares;
using IczpNet.Chat.SquareSections.Squares.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Services
{
    public class SquareAppService
        : CrudChatAppService<
            Square,
            SquareDetailDto,
            SquareDto,
            Guid,
            SquareGetListInput,
            SquareCreateInput,
            SquareUpdateInput>,
        ISquareAppService
    {

        protected ISquareCategoryManager SquareCategoryManager { get; }

        public SquareAppService(IRepository<Square, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<Square>> CreateFilteredQueryAsync(SquareGetListInput input)
        {
            //Category
            IQueryable<Guid> categoryIdQuery = null;

            if (input.IsImportChildCategory && input.CategoryIdList.IsAny())
            {
                categoryIdQuery = (await SquareCategoryManager.QueryCurrentAndAllChildsAsync(input.CategoryIdList)).Select(x => x.Id);
            }

            return (await ReadOnlyRepository.GetQueryableAsync())
                //CategoryId
                .WhereIf(!input.IsImportChildCategory && input.CategoryIdList.IsAny(), x => input.CategoryIdList.Contains(x.CategoryId.Value))
                .WhereIf(input.IsImportChildCategory && input.CategoryIdList.IsAny(), x => categoryIdQuery.Contains(x.CategoryId.Value))
                .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Name.Contains(input.Keyword) || x.Code.Contains(input.Keyword));
        }
    }
}
