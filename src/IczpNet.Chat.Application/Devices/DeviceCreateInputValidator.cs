using FluentValidation;
using IczpNet.Chat.Localization;
using IczpNet.Chat.Devices;
using Microsoft.Extensions.Localization;

namespace IczpNet.Chat.Devices;

public class DeviceCreateInputValidator : AbstractValidator<DeviceCreateInput>
{
    public DeviceCreateInputValidator(IStringLocalizer<ChatResource> _localizer)
    {

        //RuleFor(x => x.Name)
        //   .NotEmpty()
        //   .WithMessage(_localizer["Chat"]);
    }
}
