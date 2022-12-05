using FluentValidation;
using IczpNet.Chat.RoomSections.Rooms.Dtos;

namespace IczpNet.Chat.Validators
{
    public class RoomCreateInputValidator : AbstractValidator<RoomCreateInput>
    {
        public RoomCreateInputValidator()
        {

            RuleFor(x => x.Name).NotEmpty();

            RuleFor(x => x.ChatObjectIdList).NotNull().MinCount(2);
        }
    }
}
