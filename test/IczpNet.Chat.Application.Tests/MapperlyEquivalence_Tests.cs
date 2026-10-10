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
    [Fact]
    public void Menu_output_mapping_works_properly()
    {
        var source = new Menu { OwnerId = 123, Description = "menu", Sorting = 5 };
        source.SetName("root");

        var candidate = new ChatApplicationMenuToMenuDetailDtoMapper().Map(source);

        candidate.OwnerId.ShouldBe(123);
        candidate.Name.ShouldBe("root");
        candidate.Description.ShouldBe("menu");
    }

    [Fact]
    public void Menu_create_and_update_input_mappings_work_properly()
    {
        var parentId = Guid.NewGuid();
        var createInput = new MenuCreateInput
        {
            Name = "first",
            OwnerId = 123,
            Description = "original",
            ParentId = parentId,
            Sorting = 3,
        };
        var candidate = new ChatApplicationMenuCreateInputToMenuMapper().Map(createInput);
        candidate.Name.ShouldBe("first");
        candidate.OwnerId.ShouldBe(123);
        candidate.Description.ShouldBe("original");
        candidate.ParentId.ShouldBe(parentId);
        candidate.Sorting.ShouldBe(3);

        var updateInput = new MenuUpdateInput
        {
            Name = "second",
            Description = "updated",
            ParentId = null,
            Sorting = 4,
        };
        new ChatApplicationMenuUpdateInputToMenuMapper().Map(updateInput, candidate);
        candidate.Name.ShouldBe("second");
        candidate.Description.ShouldBe("updated");
        candidate.ParentId.ShouldBeNull();
        candidate.Sorting.ShouldBe(4);
    }

    [Fact]
    public void ChatObject_create_input_mapping_works_properly()
    {
        var source = CreateChatObjectInput();
        var candidate = new ChatApplicationChatObjectCreateInputToChatObjectMapper().Map(source);

        candidate.Name.ShouldBe("tester");
        candidate.Description.ShouldBe("profile");
        candidate.Gender.ShouldBe(Genders.Female);
        candidate.VerificationMethod.ShouldBe(VerificationMethods.Required);
        candidate.ChatObjectTypeId.ShouldBe("personal");
        candidate.ObjectType.ShouldBe(ChatObjectTypeEnums.Personal);
        candidate.ParentId.ShouldBe(23);
        candidate.Sorting.ShouldBe(5);
    }

    [Fact]
    public void ChatObject_update_input_mapping_works_properly()
    {
        var createInput = CreateChatObjectInput();
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

        new ChatApplicationChatObjectUpdateInputToChatObjectMapper().Map(updateInput, candidate);

        candidate.Name.ShouldBe("renamed");
        candidate.Description.ShouldBe("updated");
        candidate.Gender.ShouldBe(Genders.Male);
        candidate.VerificationMethod.ShouldBe(VerificationMethods.Rejected);
        candidate.ParentId.ShouldBe(45);
        candidate.Sorting.ShouldBe(7);
    }

    [Fact]
    public void ChatObjectType_create_and_update_input_mappings_work_properly()
    {
        var createInput = new ChatObjectTypeCreateInput
        {
            Id = "personal",
            Name = "Personal",
            Description = "original",
            MaxDepth = 2,
            IsHasChild = true,
        };
        var candidate = new ChatApplicationChatObjectTypeCreateInputToChatObjectTypeMapper().Map(createInput);
        candidate.Id.ShouldBe("personal");
        candidate.Name.ShouldBe("Personal");
        candidate.Description.ShouldBe("original");
        candidate.MaxDepth.ShouldBe(2);
        candidate.IsHasChild.ShouldBeTrue();

        var updateInput = new ChatObjectTypeUpdateInput
        {
            Name = "New name",
            Description = "updated",
            MaxDepth = 3,
            IsHasChild = false,
        };
        new ChatApplicationChatObjectTypeUpdateInputToChatObjectTypeMapper().Map(updateInput, candidate);
        candidate.Name.ShouldBe("New name");
        candidate.Description.ShouldBe("updated");
        candidate.MaxDepth.ShouldBe(3);
        candidate.IsHasChild.ShouldBeFalse();
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
}
