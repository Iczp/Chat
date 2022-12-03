using FluentValidation;
using IczpNet.Chat.RoomSections.RoomRoles.Dtos;
using IczpNet.Chat.RoomSections.Rooms;

namespace IczpNet.Chat.Validators
{
    public class RoomRoleCreateInputValidator : AbstractValidator<RoomRoleCreateInput>
    {
        public RoomRoleCreateInputValidator()
        {
            RuleFor(x => x.Name).NotEmpty();

            RuleFor(x => x.RoomId).NotEmpty().WithMessage($"{nameof(RoomRoleCreateInput.RoomId)} required.");
        }
    }
}
