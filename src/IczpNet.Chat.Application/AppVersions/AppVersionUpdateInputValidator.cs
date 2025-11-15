using FluentValidation;
using IczpNet.Chat.Localization;
using IczpNet.Chat.AppVersions;
using Microsoft.Extensions.Localization;

namespace IczpNet.Chat.AppVersions;

public class AppVersionUpdateInputValidator : AbstractValidator<AppVersionUpdateInput>
{
    public AppVersionUpdateInputValidator(IStringLocalizer<ChatResource> _localizer)
    {

        //RuleFor(x => x.Name)
        //   .NotEmpty()
        //   .WithMessage(_localizer["Chat"]);
    }
}
