using AutoMapper;
using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.ChatObjectTypes.Dtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Menus;
using IczpNet.Chat.Menus.Dtos;
using System;
using Shouldly;
using Xunit;

namespace IczpNet.Chat;

public class MapperlyEquivalence_Tests
{
    private readonly IMapper _legacyMapper;

    public MapperlyEquivalence_Tests()
    {
        var profile = typeof(ChatApplicationModule).Assembly.GetType(
            "IczpNet.Chat.AutoMappers.ChatApplicationAutoMapperProfile", throwOnError: true)!;
        var configuration = new MapperConfiguration(config => config.AddProfile(profile));
        _legacyMapper = configuration.CreateMapper();
    }

    [Fact]
    public void Menu_output_mapping_matches_the_current_provider()
    {
        var source = new Menu { OwnerId = 123, Description = "menu", Sorting = 5 };
        source.SetName("root");

        var legacy = _legacyMapper.Map<MenuDetailDto>(source);
        var candidate = new ChatApplicationMenuToMenuDetailDtoMapper().Map(source);

        candidate.OwnerId.ShouldBe(legacy.OwnerId);
        candidate.Name.ShouldBe(legacy.Name);
        candidate.Description.ShouldBe(legacy.Description);
    }

    [Fact]
    public void Menu_create_and_update_input_mappings_match_the_current_provider()
    {
        var createInput = new MenuCreateInput
        {
            Name = "first",
            OwnerId = 123,
            Description = "original",
            ParentId = Guid.NewGuid(),
            Sorting = 3,
        };
        var legacy = _legacyMapper.Map<Menu>(createInput);
        var candidate = new ChatApplicationMenuCreateInputToMenuMapper().Map(createInput);
        AssertSameMenuFields(legacy, candidate);

        var updateInput = new MenuUpdateInput
        {
            Name = "second",
            Description = "updated",
            ParentId = null,
            Sorting = 4,
        };
        _legacyMapper.Map(updateInput, legacy);
        new ChatApplicationMenuUpdateInputToMenuMapper().Map(updateInput, candidate);
        AssertSameMenuFields(legacy, candidate);
    }

    [Fact]
    public void ChatObject_create_input_mapping_matches_the_current_provider()
    {
        var source = CreateChatObjectInput();
        var legacy = _legacyMapper.Map<ChatObject>(source);
        var candidate = new ChatApplicationChatObjectCreateInputToChatObjectMapper().Map(source);

        AssertSameChatObjectFields(legacy, candidate);
    }

    [Fact]
    public void ChatObject_update_input_mapping_matches_the_current_provider()
    {
        var createInput = CreateChatObjectInput();
        var legacy = _legacyMapper.Map<ChatObject>(createInput);
        var candidate = new ChatApplicationChatObjectCreateInputToChatObjectMapper().Map(createInput);
        var updateInput = new ChatObjectUpdateInput
        {
            Name = "renamed",
            Description = "updated",
            Gender = Genders.Male,
            VerificationMethod = VerificationMethods.Rejected,
            ParentId = 45,
            Sorting = 7,
        };

        _legacyMapper.Map(updateInput, legacy);
        new ChatApplicationChatObjectUpdateInputToChatObjectMapper().Map(updateInput, candidate);

        AssertSameChatObjectFields(legacy, candidate);
    }

    [Fact]
    public void ChatObjectType_create_and_update_input_mappings_match_the_current_provider()
    {
        var createInput = new ChatObjectTypeCreateInput
        {
            Id = "personal",
            Name = "Personal",
            Description = "original",
            MaxDepth = 2,
            IsHasChild = true,
        };
        var legacy = _legacyMapper.Map<ChatObjectType>(createInput);
        var candidate = new ChatApplicationChatObjectTypeCreateInputToChatObjectTypeMapper().Map(createInput);
        AssertSameChatObjectTypeFields(legacy, candidate);

        var updateInput = new ChatObjectTypeUpdateInput
        {
            Name = "New name",
            Description = "updated",
            MaxDepth = 3,
            IsHasChild = false,
        };
        _legacyMapper.Map(updateInput, legacy);
        new ChatApplicationChatObjectTypeUpdateInputToChatObjectTypeMapper().Map(updateInput, candidate);
        AssertSameChatObjectTypeFields(legacy, candidate);
    }

    private static ChatObjectCreateInput CreateChatObjectInput() => new()
    {
        Name = "tester",
        Description = "profile",
        Gender = Genders.Female,
        VerificationMethod = VerificationMethods.Required,
        ChatObjectTypeId = "personal",
        ObjectType = ChatObjectTypeEnums.Personal,
        ParentId = 23,
        Sorting = 5,
    };

    private static void AssertSameChatObjectFields(ChatObject expected, ChatObject actual)
    {
        actual.Name.ShouldBe(expected.Name);
        actual.NameSpelling.ShouldBe(expected.NameSpelling);
        actual.NameSpellingAbbreviation.ShouldBe(expected.NameSpellingAbbreviation);
        actual.Description.ShouldBe(expected.Description);
        actual.Gender.ShouldBe(expected.Gender);
        actual.VerificationMethod.ShouldBe(expected.VerificationMethod);
        actual.ChatObjectTypeId.ShouldBe(expected.ChatObjectTypeId);
        actual.ObjectType.ShouldBe(expected.ObjectType);
        actual.ParentId.ShouldBe(expected.ParentId);
        actual.Sorting.ShouldBe(expected.Sorting);
        actual.IsEnabled.ShouldBe(expected.IsEnabled);
        actual.StateCheckers.Count.ShouldBe(expected.StateCheckers.Count);
    }

    private static void AssertSameMenuFields(Menu expected, Menu actual)
    {
        actual.Name.ShouldBe(expected.Name);
        actual.OwnerId.ShouldBe(expected.OwnerId);
        actual.Description.ShouldBe(expected.Description);
        actual.ParentId.ShouldBe(expected.ParentId);
        actual.Sorting.ShouldBe(expected.Sorting);
        actual.Id.ShouldBe(expected.Id);
    }

    private static void AssertSameChatObjectTypeFields(ChatObjectType expected, ChatObjectType actual)
    {
        actual.Id.ShouldBe(expected.Id);
        actual.Name.ShouldBe(expected.Name);
        actual.Description.ShouldBe(expected.Description);
        actual.MaxDepth.ShouldBe(expected.MaxDepth);
        actual.IsHasChild.ShouldBe(expected.IsHasChild);
        actual.IsStatic.ShouldBe(expected.IsStatic);
        actual.ObjectType.ShouldBe(expected.ObjectType);
    }
}
