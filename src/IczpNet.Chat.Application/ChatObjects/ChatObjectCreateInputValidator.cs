using FluentValidation;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Localization;
using IczpNet.Chat.Validators;
using Volo.Abp.Localization;

namespace IczpNet.Chat.ChatObjects
{
    public class ChatObjectCreateInputValidator : AbstractValidator<ChatObjectCreateInput>
    {
        public ChatObjectCreateInputValidator()
        {
            RuleFor(x => x.Description).Url();

            RuleFor(x => x.Name).NotNull().NotEmpty();

            RuleFor(x => x.ChatObjectTypeId).NotNull().NotEmpty();

            RuleFor(x => x.ObjectType).NotNull().NotEmpty().IsInEnum();

            RuleFor(x => x.Sorting).Equal(0).WithMessage("Sorting must be:0");
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<ChatResource>(name);
        }
    }
}
