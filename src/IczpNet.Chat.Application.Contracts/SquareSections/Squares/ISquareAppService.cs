using IczpNet.Chat.SquareSections.Squares.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.SquareSections.Squares;

public interface ISquareAppService :
    ICrudAppService<
        SquareDetailDto,
        SquareDto,
        Guid,
        SquareGetListInput,
        SquareCreateInput,
        SquareUpdateInput>
{
}
