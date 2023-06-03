using AutoMapper;
using IczpNet.Chat.FriendshipTagSections.FriendshipTags.Dtos;
using IczpNet.Chat.SessionSections.FriendshipRequests;
using IczpNet.Chat.SessionSections.FriendshipRequests.Dtos;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.Friendships.Dtos;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.Sessions.Dtos;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using IczpNet.Chat.SessionServices;
using IczpNet.Chat.SessionSections.SessionOrganizations;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using IczpNet.Chat.SessionSections.SessionTags.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionSections.SessionPermissions.Dtos;
using IczpNet.Chat.SessionSections.SessionRequests.Dtos;
using IczpNet.Chat.SessionSections.SessionRequests;
using IczpNet.Chat.SessionSections.SessionPermissionGroups;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.OpenedRecorders.Dtos;
using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.FavoritedRecorders.Dtos;
using IczpNet.Chat.SessionSections.SessionUnitSettings;
using IczpNet.Chat.SessionUnits;

namespace IczpNet.Chat.AutoMappers;

public class SessionSectionApplicationAutoMapperProfile : Profile
{
    public SessionSectionApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */


        //Session
        CreateMap<Session, SessionDto>();
        CreateMap<Session, SessionDetailDto>();

        //SessionUnit
        CreateMap<SessionUnit, SessionUnitDto>();
        CreateMap<SessionUnit, SessionUnitOwnerDto>().AfterMap<SessionUnitOwnerDtoMappingAction>();
        CreateMap<SessionUnit, SessionUnitDestinationDetailDto>();
        CreateMap<SessionUnit, SessionUnitDestinationDto>();
        CreateMap<SessionUnit, SessionUnitWithDestinationDto>();
        //CreateMap<SessionUnitModel, SessionUnitOwnerDto>();

        CreateMap<SessionUnitSetting, SessionUnitSettingDto>();

        //SessionOrganization
        CreateMap<SessionOrganization, SessionOrganizationDto>();
        CreateMap<SessionOrganization, SessionOrganizationDetailDto>();
        CreateMap<SessionOrganizationCreateInput, SessionOrganization>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<SessionOrganizationCreateBySessionUnitInput, SessionOrganization>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<SessionOrganizationUpdateInput, SessionOrganization>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<SessionOrganization, SessionOrganizationInfo>();

        //SessionTag
        CreateMap<SessionTag, SessionTagDto>();
        CreateMap<SessionTag, SessionTagDetailDto>();
        CreateMap<SessionTag, SessionTagSimpleDto>();
        CreateMap<SessionTagCreateInput, SessionTag>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<SessionTagUpdateInput, SessionTag>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        //SessionRole
        CreateMap<SessionRole, SessionRoleDto>();
        CreateMap<SessionRole, SessionRolePermissionDto>();
        CreateMap<SessionRole, SessionRoleDetailDto>();
        CreateMap<SessionRole, SessionRoleSimpleDto>();
        CreateMap<SessionRoleCreateInput, SessionRole>(MemberList.None)
            .IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
            .IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<SessionRoleCreateBySessionUnitInput, SessionRole>(MemberList.None)
            .IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
            .IgnoreAllPropertiesWithAnInaccessibleSetter();
        CreateMap<SessionRoleUpdateInput, SessionRole>(MemberList.None)
            .IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
            .IgnoreAllPropertiesWithAnInaccessibleSetter();

        //PermissionGrant
        CreateMap<SessionPermissionRoleGrant, PermissionGrantValue>();
        CreateMap<SessionPermissionUnitGrant, PermissionGrantValue>();
        CreateMap<SessionPermissionRoleGrant, SessionPermissionRoleGrantDto>();
        CreateMap<SessionPermissionUnitGrant, SessionPermissionUnitGrantDto>();

        //SessionPermissionDefinition
        CreateMap<SessionPermissionDefinition, SessionPermissionDefinitionDto>();
        CreateMap<SessionPermissionDefinition, SessionPermissionDefinitionDetailDto>();
        CreateMap<SessionPermissionDefinition, SessionPermissionDefinitionSimpleDto>();
        CreateMap<SessionPermissionDefinitionCreateInput, SessionPermissionDefinition>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<SessionPermissionDefinitionUpdateInput, SessionPermissionDefinition>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        //SessionPermissionGroup
        CreateMap<SessionPermissionGroup, SessionPermissionGroupDto>();
        CreateMap<SessionPermissionGroup, SessionPermissionGroupDetailDto>();
        CreateMap<SessionPermissionGroupCreateInput, SessionPermissionGroup>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<SessionPermissionGroupUpdateInput, SessionPermissionGroup>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<SessionPermissionGroup, SessionPermissionGroupInfo>();


        //SessionRequest
        CreateMap<SessionRequest, SessionRequestDto>();
        CreateMap<SessionRequest, SessionRequestDetailDto>();
        CreateMap<SessionRequestCreateInput, SessionRequest>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<SessionRequestUpdateInput, SessionRequest>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        //Friendship
        CreateMap<Friendship, FriendshipDto>();
        CreateMap<Friendship, FriendshipDetailDto>();
        CreateMap<FriendshipCreateInput, Friendship>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<FriendshipUpdateInput, Friendship>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        //FriendshipRequest
        CreateMap<FriendshipRequest, FriendshipRequestDto>();
        CreateMap<FriendshipRequest, FriendshipRequestDetailDto>().ForMember(x => x.FriendshipIdList, o => o.MapFrom(x => x.GetFriendshipIdList()));
        CreateMap<FriendshipRequestCreateInput, FriendshipRequest>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<FriendshipRequestUpdateInput, FriendshipRequest>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

        //FriendshipTag
        CreateMap<FriendshipTag, FriendshipTagDto>();
        CreateMap<FriendshipTag, FriendshipTagDetailDto>();
        CreateMap<FriendshipTag, FriendshipTagSimpleDto>();
        CreateMap<FriendshipTagCreateInput, FriendshipTag>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        CreateMap<FriendshipTagUpdateInput, FriendshipTag>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();


        //OpenedRecorder
        CreateMap<OpenedRecorder, OpenedRecorderDto>();

        //FavoritedRecorder
        CreateMap<FavoritedRecorder, FavoritedRecorderDto>();
    }
}
