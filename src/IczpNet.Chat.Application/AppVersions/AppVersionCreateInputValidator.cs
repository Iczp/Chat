using FluentValidation;
using IczpNet.Chat.Localization;
using IczpNet.Chat.AppVersions;
using Microsoft.Extensions.Localization;

namespace IczpNet.Chat.AppVersions;

public class AppVersionCreateInputValidator : AbstractValidator<AppVersionCreateInput>
{
    public AppVersionCreateInputValidator(IStringLocalizer<ChatResource> _localizer)
    {

        //RuleFor(x => x.Name)
        //   .NotEmpty()
        //   .WithMessage(_localizer["Chat"]);
    }
}
