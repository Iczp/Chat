using FluentValidation;
using IczpNet.Chat.ChatObjects.Dtos;

namespace IczpNet.Chat.ChatObjects
{
    public class ChatObjectCreateInputValidator : AbstractValidator<ChatObjectCreateInput>
    {
        public ChatObjectCreateInputValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();

            RuleFor(x => x.ChatObjectTypeId).NotNull().NotEmpty();

            RuleFor(x => x.ObjectType).NotNull().NotEmpty();

            RuleFor(x => x.Sorting).Equal(0).WithMessage("Sorting must be:0");
        }
    }
}
