using FluentValidation;
using IczpNet.Chat.Localization;
using IczpNet.Chat.SessionBoxes;
using Microsoft.Extensions.Localization;

namespace IczpNet.Chat.SessionBoxes;

public class BoxUpdateInputValidator : AbstractValidator<BoxUpdateInput>
{
    public BoxUpdateInputValidator(IStringLocalizer<ChatResource> _localizer)
    {

        //RuleFor(x => x.Name)
        //   .NotEmpty()
        //   .WithMessage(_localizer["Chat"]);
    }
}
