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
            IQueryable<Guid> positionIdQuery = null;

            if (input.IsImportChildCategory && input.SquareCategoryIdList.IsAny())
            {
                positionIdQuery = (await SquareCategoryManager.QueryCurrentAndAllChildsAsync(input.SquareCategoryIdList)).Select(x => x.Id);
            }

            return (await ReadOnlyRepository.GetQueryableAsync())

                //CategoryId
                .WhereIf(!input.IsImportChildCategory && input.SquareCategoryIdList.IsAny(), x => input.SquareCategoryIdList.Contains(x.SquareCategoryId.Value))
                .WhereIf(input.IsImportChildCategory && input.SquareCategoryIdList.IsAny(), x => positionIdQuery.Contains(x.SquareCategoryId.Value))
                .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Name.Contains(input.Keyword) || x.Code.Contains(input.Keyword));
        }
    }
}
