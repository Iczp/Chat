using AutoMapper;
using IczpNet.Chat.SquareSections.SquareCategorys;
using IczpNet.Chat.SquareSections.SquareCategorys.Dtos;
using IczpNet.Chat.SquareSections.Squares;
using IczpNet.Chat.SquareSections.Squares.Dtos;

namespace IczpNet.Chat.AutoMappers;

public class SquareSectionApplicationAutoMapperProfile : Profile
{
    public SquareSectionApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        //SquareCategory
        CreateMap<SquareCategory, SquareCategoryDto>();
        CreateMap<SquareCategory, SquareCategoryDetailDto>();
        CreateMap<SquareCategoryCreateInput, SquareCategory>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<SquareCategoryUpdateInput, SquareCategory>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<SquareCategory, SquareCategoryInfo>();

        //Square
        CreateMap<Square, SquareDto>().ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()));
        CreateMap<Square, SquareDetailDto>().ForMember(x => x.MemberCount, o => o.MapFrom(x => x.GetMemberCount()));
        CreateMap<SquareCreateInput, Square>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<SquareUpdateInput, Square>(MemberList.Source).IgnoreAllPropertiesWithAnInaccessibleSetter();
    }
}
